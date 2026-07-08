using System.Collections.Generic;
using _Scripts.Simulation.Jobs;
using UnityEngine;

/// <summary>
/// Drives a dweller through a single <see cref="MiningJob"/>: path to a cell from which the
/// block can be mined, stand there for the job's work duration, then remove the block.
///
///   Pathing -> build an A* path to the closest mining stand cell (fails if none reachable)
///   Moving  -> let locomotion walk the path; fail if it reports itself blocked
///   Working -> face the block and count down the work timer, then complete the job
/// </summary>
public class MiningTask : IDwellerTask
{
    private enum Step { Pathing, Moving, Working }

    private readonly Job _job;
    private Step _step = Step.Pathing;
    private float _workTimer;

    public MiningTask(Job job)
    {
        _job = job;
    }

    /// <summary>The job this task is carrying out (used by the scheduler to blacklist on failure).</summary>
    public Job Job => _job;

    public TaskStatus Tick(DwellerContext ctx)
    {
        switch (_step)
        {
            case Step.Pathing:
                return BeginPath(ctx);

            case Step.Moving:
                if (ctx.Locomotion.IsBlocked) return TaskStatus.Failed;
                if (ctx.Locomotion.IsMoving) return TaskStatus.Running;
                // Path fully walked (or was already empty): drop into work.
                _step = Step.Working;
                return Work(ctx);

            case Step.Working:
                return Work(ctx);
        }

        return TaskStatus.Running;
    }

    private TaskStatus BeginPath(DwellerContext ctx)
    {
        Vector3Int myCell = ctx.Locomotion.CurrentCell();
        if (!GridPathfinder.TryGetStandableCell(myCell, out Vector3Int start))
        {
            start = myCell;
        }

        List<Vector3Int> standCells = GridPathfinder.GetMiningStandCells(_job.Position);
        List<Vector3Int> path = standCells.Count > 0
            ? GridPathfinder.FindPath(start, standCells, 4000, DwellerOccupancy.GetBlockedCells(ctx.Agent))
            : null;

        // Cannot reach this job right now; the coordinator will blacklist it briefly.
        if (path == null) return TaskStatus.Failed;

        ctx.Locomotion.FollowPath(path);
        _step = Step.Moving;
        return TaskStatus.Running;
    }

    private TaskStatus Work(DwellerContext ctx)
    {
        // The block may have vanished already (e.g. a neighbouring tree was felled).
        if (!IsStillMineable(_job.Position))
        {
            JobManager.Instance.CompleteJob(_job);
            return TaskStatus.Succeeded;
        }

        ctx.Locomotion.FaceTowards(new Vector3(_job.Position.x, ctx.Agent.transform.position.y, _job.Position.z));

        _workTimer += Time.deltaTime;
        if (_workTimer >= _job.WorkDuration)
        {
            JobManager.Instance.CompleteJob(_job);
            return TaskStatus.Succeeded;
        }

        return TaskStatus.Running;
    }

    public void OnAbort(DwellerContext ctx)
    {
        // Hand the job back so it (or a better-placed dweller) can reclaim it.
        JobManager.Instance.AbandonJob(_job);
    }

    private static bool IsStillMineable(Vector3Int position)
    {
        return BlockUtils.IsSolid(position.x, position.y, position.z);
    }
}

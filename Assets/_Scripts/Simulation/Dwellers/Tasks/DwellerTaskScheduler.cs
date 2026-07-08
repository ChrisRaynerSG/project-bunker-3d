using System.Collections.Generic;
using _Scripts.Simulation.Jobs;
using UnityEngine;

/// <summary>
/// Decides what a dweller should do next. Today it claims the closest reachable mining job and
/// wraps it in a <see cref="MiningTask"/>; as new job types are added this is the one place that
/// grows (map a job to the task that carries it out).
///
/// It also owns the per-dweller "unreachable" blacklist: jobs that could not be reached or were
/// given up on are avoided for a short while so the dweller tries other work first, then retried
/// once the interval elapses (positions may have opened up as the world changed).
/// </summary>
public class DwellerTaskScheduler
{
    private readonly HashSet<Vector3Int> _unreachable = new HashSet<Vector3Int>();
    private readonly float _blacklistRetryInterval;
    private float _blacklistTimer;

    public DwellerTaskScheduler(float blacklistRetryInterval)
    {
        _blacklistRetryInterval = blacklistRetryInterval;
    }

    /// <summary>Retires the unreachable blacklist over time so jobs get retried.</summary>
    public void Tick(float deltaTime)
    {
        if (_unreachable.Count == 0) return;

        _blacklistTimer += deltaTime;
        if (_blacklistTimer >= _blacklistRetryInterval)
        {
            _blacklistTimer = 0f;
            _unreachable.Clear();
        }
    }

    /// <summary>Claims the closest job the dweller is willing to attempt, or null if there is none.</summary>
    public IDwellerTask TrySelectTask(DwellerContext ctx)
    {
        Vector3Int myCell = ctx.Locomotion.CurrentCell();
        Job job = JobManager.Instance.TryClaimClosestJob(myCell, _unreachable);
        if (job == null) return null;

        return new MiningTask(job);
    }

    /// <summary>Records that a task failed so its target is avoided until the blacklist retires.</summary>
    public void ReportFailed(IDwellerTask task)
    {
        if (task is MiningTask mining)
        {
            _unreachable.Add(mining.Job.Position);
        }
    }

    /// <summary>
    /// Clears the blacklist because the world changed under us (a job completed, or we landed
    /// somewhere new), so previously unreachable jobs may now be reachable.
    /// </summary>
    public void ClearBlacklist()
    {
        _unreachable.Clear();
    }
}

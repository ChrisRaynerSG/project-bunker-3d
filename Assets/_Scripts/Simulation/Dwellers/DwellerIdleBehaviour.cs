using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives a dweller something to do while it has no task: every so often it either does a little
/// in-place hop or wanders to a random nearby standable cell. Purely cosmetic — it drives the
/// same <see cref="DwellerLocomotion"/> the tasks use.
/// </summary>
public class DwellerIdleBehaviour
{
    private readonly DwellerIdleSettings _settings;

    private float _idleActionTimer;
    private float _nextIdleActionDelay;

    public DwellerIdleBehaviour(DwellerIdleSettings settings)
    {
        _settings = settings;
        ScheduleNextAction();
    }

    public void Tick(DwellerContext ctx)
    {
        // Let an in-progress wander finish before picking the next antic.
        if (ctx.Locomotion.IsMoving) return;

        _idleActionTimer += Time.deltaTime;
        if (_idleActionTimer < _nextIdleActionDelay) return;

        ScheduleNextAction();

        if (Random.value < _settings.idleHopChance)
        {
            BeginIdleHop(ctx);
            return;
        }

        TryBeginIdleWander(ctx);
    }

    private void BeginIdleHop(DwellerContext ctx)
    {
        Vector2 direction = Random.insideUnitCircle;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }

        ctx.Locomotion.BeginIdleHop(
            new Vector3(direction.x, 0f, direction.y),
            _settings.idleHopHeight,
            _settings.idleHopDuration);
    }

    private bool TryBeginIdleWander(DwellerContext ctx)
    {
        Vector3Int origin = ctx.Locomotion.CurrentCell();
        List<Vector3Int> candidates = new List<Vector3Int>();
        int radius = _settings.idleWanderRadius;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dz = -radius; dz <= radius; dz++)
            {
                int manhattan = Mathf.Abs(dx) + Mathf.Abs(dz);
                if (manhattan == 0 || manhattan > radius) continue;

                for (int dy = 1; dy >= -1; dy--)
                {
                    Vector3Int candidate = new Vector3Int(origin.x + dx, origin.y + dy, origin.z + dz);
                    if (!GridPathfinder.IsStandable(candidate) || candidates.Contains(candidate)) continue;
                    candidates.Add(candidate);
                }
            }
        }

        while (candidates.Count > 0)
        {
            int index = Random.Range(0, candidates.Count);
            Vector3Int destination = candidates[index];
            candidates.RemoveAt(index);

            List<Vector3Int> path = GridPathfinder.FindPath(origin, new[] { destination }, 256,
                DwellerOccupancy.GetBlockedCells(ctx.Agent));
            if (path == null || path.Count == 0) continue;

            ctx.Locomotion.FollowPath(path);
            return true;
        }

        return false;
    }

    private void ScheduleNextAction()
    {
        _idleActionTimer = 0f;
        float minDelay = Mathf.Min(_settings.idleActionIntervalRange.x, _settings.idleActionIntervalRange.y);
        float maxDelay = Mathf.Max(_settings.idleActionIntervalRange.x, _settings.idleActionIntervalRange.y);
        _nextIdleActionDelay = Random.Range(minDelay, maxDelay);
    }
}

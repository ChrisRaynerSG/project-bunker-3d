using System;
using UnityEngine;

/// <summary>
/// Tunables for how a dweller physically moves over the grid. Consumed by
/// <see cref="DwellerLocomotion"/>. Grouped into a serializable class so it still shows as a
/// foldout in the Inspector while keeping the fields off the coordinator component.
/// </summary>
[Serializable]
public class DwellerLocomotionSettings
{
    [Header("Movement")]
    [Tooltip("Horizontal walking speed in blocks per (simulation) second.")]
    public float moveSpeed = 3f;

    [Tooltip("How quickly the dweller turns to face its direction of travel / work.")]
    public float turnSpeed = 12f;

    [Tooltip("Vertical offset added on top of the standable cell so the model rests nicely.")]
    public float groundYOffset = 0f;

    [Header("Hop")]
    [Tooltip("Peak height of the hop above the take-off cell, in blocks. ~1.25 clears a one-block ledge with a little air to spare.")]
    public float hopHeight = 1.25f;

    [Tooltip("Seconds the hop takes from take-off to landing (in simulation time).")]
    public float hopDuration = 0.28f;

    [Header("Falling")]
    [Tooltip("Downward acceleration while falling, in blocks per (simulation) second squared.")]
    public float fallGravity = 25f;

    [Tooltip("Maximum downward speed while falling, in blocks per (simulation) second.")]
    public float maxFallSpeed = 30f;

    [Header("Behaviour")]
    [Tooltip("Distance (blocks) at which a waypoint is considered reached.")]
    public float waypointTolerance = 0.06f;

    [Tooltip("Seconds of no progress before the mover reports itself blocked.")]
    public float stuckTimeout = 4f;
}

/// <summary>
/// Tunables for the idle antics a dweller performs when it has no work to do. Consumed by
/// <see cref="DwellerIdleBehaviour"/>.
/// </summary>
[Serializable]
public class DwellerIdleSettings
{
    [Tooltip("Seconds between idle antics when no jobs are available. X = min, Y = max.")]
    public Vector2 idleActionIntervalRange = new Vector2(1.2f, 2.8f);

    [Tooltip("Chance that an idle action is a little in-place hop instead of a short wander.")]
    [Range(0f, 1f)]
    public float idleHopChance = 0.3f;

    [Tooltip("How many blocks from its current cell an idle dweller may wander.")]
    [Min(1f)]
    public int idleWanderRadius = 5;

    [Tooltip("Peak height of the little idle hop, in blocks.")]
    public float idleHopHeight = 0.35f;

    [Tooltip("Seconds the little idle hop takes from take-off to landing (in simulation time).")]
    public float idleHopDuration = 0.22f;
}

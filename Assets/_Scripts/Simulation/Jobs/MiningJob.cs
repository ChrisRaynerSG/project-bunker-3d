using _Scripts.Simulation.Jobs;
using UnityEngine;

/// <summary>
/// A single unit of work: a request to mine (remove) the solid block at a specific
/// world position. Jobs are created by the player (via the mining selection) and
/// claimed, walked to, and completed by dwellers.
/// </summary>
public class MiningJob : Job
{
    /// <summary>The world position of the block to be mined.</summary>
    public Vector3Int Position { get; }

    /// <summary>How long a dweller must "work" the block before it is removed (seconds).</summary>
    public float WorkDuration { get; }

    /// <summary>True once a dweller has taken responsibility for this job.</summary>
    public bool IsClaimed { get; set; }

    /// <summary>Optional world-space marker shown while the job is pending.</summary>
    public GameObject Marker { get; set; }

    public MiningJob(Vector3Int position, float workDuration) : base(position, workDuration)
    {
        Position = position;
        WorkDuration = Mathf.Max(0.1f, workDuration);
        IsClaimed = false;
    }
}

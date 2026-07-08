/// <summary>
/// Result of ticking an <see cref="IDwellerTask"/> for one frame.
/// </summary>
public enum TaskStatus
{
    /// <summary>Still working on it; tick again next frame.</summary>
    Running,

    /// <summary>Finished successfully; the coordinator drops the task.</summary>
    Succeeded,

    /// <summary>Could not be completed (unreachable, blocked, invalid); the coordinator drops and reports it.</summary>
    Failed
}

/// <summary>
/// A self-contained unit of dweller behaviour (mine a block, eat, craft, …). A task drives the
/// dweller by issuing commands to its <see cref="DwellerLocomotion"/> and reports progress via
/// <see cref="Tick"/>. Adding a new kind of work means writing a new task class and having the
/// scheduler hand it out — <see cref="DwellerAgent"/> itself never changes.
/// </summary>
public interface IDwellerTask
{
    /// <summary>Advances the task by one frame and reports whether it is done.</summary>
    TaskStatus Tick(DwellerContext ctx);

    /// <summary>
    /// Called when the task is torn down before it succeeded (e.g. the dweller fell, or the task
    /// failed). Implementations release any claimed resources here (hand a job back, etc.).
    /// </summary>
    void OnAbort(DwellerContext ctx);
}

/// <summary>
/// Everything a task needs to act, without reaching into the coordinator's internals. Future
/// systems (inventory, needs, social) can be surfaced here as they are added.
/// </summary>
public class DwellerContext
{
    public DwellerAgent Agent { get; }
    public DwellerLocomotion Locomotion { get; }

    public DwellerContext(DwellerAgent agent, DwellerLocomotion locomotion)
    {
        Agent = agent;
        Locomotion = locomotion;
    }
}

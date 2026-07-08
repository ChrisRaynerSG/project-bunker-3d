using UnityEngine;

/// <summary>
/// Coordinates a single dweller. It owns the reusable <see cref="DwellerLocomotion"/> mover and
/// runs one <see cref="IDwellerTask"/> at a time, chosen by the <see cref="DwellerTaskScheduler"/>.
/// When no task is available the <see cref="DwellerIdleBehaviour"/> makes it wander or hop.
///
/// The class deliberately holds no job- or movement-specific logic: adding a new kind of work
/// (eating, crafting, …) means writing a new <see cref="IDwellerTask"/> and teaching the
/// scheduler to hand it out — this coordinator does not change.
///
/// Everything runs off the simulation-scaled <see cref="Time.deltaTime"/>, so dwellers freeze
/// while the simulation is paused and speed up with the simulation speed.
/// </summary>
public class DwellerAgent : MonoBehaviour, IUpdatable
{
    [Header("Locomotion")]
    [SerializeField] private DwellerLocomotionSettings locomotionSettings = new DwellerLocomotionSettings();

    [Header("Idle")]
    [SerializeField] private DwellerIdleSettings idleSettings = new DwellerIdleSettings();

    [Header("Jobs")]
    [Tooltip("Seconds between retrying jobs that were previously unreachable.")]
    [SerializeField] private float blacklistRetryInterval = 5f;

    private DwellerLocomotion _locomotion;
    private DwellerTaskScheduler _scheduler;
    private DwellerIdleBehaviour _idle;
    private DwellerContext _context;
    private IDwellerTask _currentTask;

    private ISimulation _simulation;
    private bool _wasFalling;

    /// <summary>The dweller's mover, exposed so tasks (via the context) can drive it.</summary>
    public DwellerLocomotion Locomotion => _locomotion;

    private void Start()
    {
        _simulation = SimulationManagerService.GetInstance();

        _locomotion = new DwellerLocomotion(this, locomotionSettings);
        _scheduler = new DwellerTaskScheduler(blacklistRetryInterval);
        _idle = new DwellerIdleBehaviour(idleSettings);
        _context = new DwellerContext(this, _locomotion);

        _locomotion.SnapToGround();
        _locomotion.ClaimCurrentCell();
    }

    private void OnEnable()
    {
        UpdateManager.Register(this);
    }

    private void OnDisable()
    {
        UpdateManager.Unregister(this);
        _locomotion?.ReleaseAllReservations();
    }

    public void OnUpdate()
    {
        if (World.Instance == null || _locomotion == null) return;

        // The simulation drives Time.timeScale (0 while paused), so nothing should move while
        // paused. Skip the AI entirely to avoid churning pathfinding.
        if (_simulation != null && _simulation.IsPaused) return;

        float deltaTime = Time.deltaTime;

        _scheduler.Tick(deltaTime);
        _locomotion.Tick();

        // Coming to rest after a fall may make previously unreachable jobs reachable again.
        if (_wasFalling && !_locomotion.IsFalling) _scheduler.ClearBlacklist();
        _wasFalling = _locomotion.IsFalling;

        // Falling overrides everything: whatever we were doing is invalid once airborne, so hand
        // any claimed job back and let the dweller re-plan once it has landed.
        if (_locomotion.IsFalling)
        {
            AbortCurrentTask();
            return;
        }

        if (_currentTask != null)
        {
            TaskStatus status = _currentTask.Tick(_context);
            if (status == TaskStatus.Failed)
            {
                _scheduler.ReportFailed(_currentTask);
                _currentTask.OnAbort(_context);
                _currentTask = null;
            }
            else if (status == TaskStatus.Succeeded)
            {
                _currentTask = null;
                // The world just changed, so previously unreachable jobs may now be reachable.
                _scheduler.ClearBlacklist();
            }

            return;
        }

        // Only look for work once the dweller is standing still — a wander or hop in progress is
        // allowed to finish first, matching the original "claim jobs only while idle" behaviour.
        if (_locomotion.IsMoving) return;

        // Nothing to do: pick up new work, or perform idle antics until some appears.
        _currentTask = _scheduler.TrySelectTask(_context);
        if (_currentTask == null)
        {
            _idle.Tick(_context);
        }
    }

    private void AbortCurrentTask()
    {
        if (_currentTask == null) return;

        _currentTask.OnAbort(_context);
        _currentTask = null;
    }
}

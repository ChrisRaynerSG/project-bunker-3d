using System.Collections.Generic;
using _Scripts.Simulation.Jobs;
using UnityEngine;

/// <summary>
/// Drives a single dweller through the mine-job loop:
///   Idle  -> claim the closest reachable <see cref="MiningJob"/> and path to it
///   Moving-> walk the A* path cell by cell
///   Working-> stand next to the block for its mining time, then remove it
///
/// Movement and work use the simulation-scaled <see cref="Time.deltaTime"/>, so
/// dwellers automatically freeze while the simulation is paused and speed up with the
/// simulation speed, matching the rest of the colony sim.
/// </summary>
public class DwellerAgent : MonoBehaviour, IUpdatable
{
    [Header("Movement")]
    [Tooltip("Horizontal walking speed in blocks per (simulation) second.")]
    public float moveSpeed = 3f;

    [Tooltip("How quickly the dweller turns to face its direction of travel / work.")]
    public float turnSpeed = 12f;

    [Tooltip("Vertical offset added on top of the standable cell so the model rests nicely.")]
    public float groundYOffset = 0f;

    [Header("Behaviour")]
    [Tooltip("Distance (blocks) at which a waypoint is considered reached.")]
    public float waypointTolerance = 0.06f;

    [Tooltip("Seconds of no progress before the dweller gives up on its current job.")]
    public float stuckTimeout = 4f;

    [Tooltip("Seconds between retrying jobs that were previously unreachable.")]
    public float blacklistRetryInterval = 5f;

    private enum State { Idle, Moving, Working }

    private State _state = State.Idle;

    private Job _currentJob;
    private List<Vector3Int> _path;
    private int _pathIndex;

    private float _workTimer;
    private float _stuckTimer;
    private float _blacklistTimer;
    private Vector3 _lastPosition;

    private readonly HashSet<Vector3Int> _unreachable = new HashSet<Vector3Int>();

    private ISimulation _simulation;

    private void Start()
    {
        _simulation = SimulationManagerService.GetInstance();
        SnapToGround();
        _lastPosition = transform.position;
    }

    private void OnEnable()
    {
        UpdateManager.Register(this);
    }

    private void OnDisable()
    {
        UpdateManager.Unregister(this);
    }

    public void OnUpdate()
    {
        if (World.Instance == null) return;

        // The simulation drives Time.timeScale (0 while paused), so nothing should
        // move while paused. Skip the AI entirely to avoid churning pathfinding.
        if (_simulation != null && _simulation.IsPaused) return;

        RetireBlacklistOverTime();

        switch (_state)
        {
            case State.Idle:
                TickIdle();
                break;
            case State.Moving:
                TickMoving();
                break;
            case State.Working:
                TickWorking();
                break;
        }
    }

    private void TickIdle()
    {
        Vector3Int myCell = CurrentCell();
        Job job = JobManager.Instance.TryClaimClosestJob(myCell, _unreachable);
        if (job == null) return;

        if (!GridPathfinder.TryGetStandableCell(myCell, out Vector3Int start))
        {
            start = myCell;
        }

        List<Vector3Int> standCells = GridPathfinder.GetMiningStandCells(job.Position);
        List<Vector3Int> path = standCells.Count > 0
            ? GridPathfinder.FindPath(start, standCells)
            : null;

        if (path == null)
        {
            // Cannot reach this job right now; hand it back and avoid re-picking it
            // until the blacklist is retried.
            JobManager.Instance.AbandonJob(job);
            _unreachable.Add(job.Position);
            return;
        }

        _currentJob = job;
        _path = path;
        _pathIndex = 0;
        _workTimer = 0f;
        _stuckTimer = 0f;
        _state = _pathIndex >= _path.Count ? State.Working : State.Moving;
    }

    private void TickMoving()
    {
        if (_currentJob == null)
        {
            _state = State.Idle;
            return;
        }

        if (_path == null || _pathIndex >= _path.Count)
        {
            _state = State.Working;
            return;
        }

        Vector3 target = CellToWorld(_path[_pathIndex]);

        FaceTowards(target);

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude <= waypointTolerance * waypointTolerance)
        {
            transform.position = target;
            _pathIndex++;
            if (_pathIndex >= _path.Count)
            {
                _state = State.Working;
            }
        }

        DetectStuck();
    }

    private void TickWorking()
    {
        if (_currentJob == null)
        {
            _state = State.Idle;
            return;
        }

        // The block may have vanished already (e.g. a neighbouring tree was felled).
        if (!IsStillMineable(_currentJob.Position))
        {
            JobManager.Instance.CompleteJob(_currentJob);
            FinishJob();
            return;
        }

        FaceTowards(new Vector3(_currentJob.Position.x, transform.position.y, _currentJob.Position.z));

        _workTimer += Time.deltaTime;
        if (_workTimer >= _currentJob.WorkDuration)
        {
            JobManager.Instance.CompleteJob(_currentJob);
            FinishJob();
        }
    }

    private void FinishJob()
    {
        _currentJob = null;
        _path = null;
        _pathIndex = 0;
        _workTimer = 0f;
        _stuckTimer = 0f;
        _state = State.Idle;

        // The world just changed, so previously unreachable jobs may now be reachable.
        _unreachable.Clear();
    }

    private void DetectStuck()
    {
        if ((transform.position - _lastPosition).sqrMagnitude > 0.0001f)
        {
            _stuckTimer = 0f;
            _lastPosition = transform.position;
            return;
        }

        _stuckTimer += Time.deltaTime;
        if (_stuckTimer >= stuckTimeout && _currentJob != null)
        {
            // Give up: return the job and blacklist it briefly so we try others first.
            JobManager.Instance.AbandonJob(_currentJob);
            _unreachable.Add(_currentJob.Position);
            _currentJob = null;
            _path = null;
            _state = State.Idle;
            _stuckTimer = 0f;
        }
    }

    private void RetireBlacklistOverTime()
    {
        if (_unreachable.Count == 0) return;

        _blacklistTimer += Time.deltaTime;
        if (_blacklistTimer >= blacklistRetryInterval)
        {
            _blacklistTimer = 0f;
            _unreachable.Clear();
        }
    }

    private bool IsStillMineable(Vector3Int position)
    {
        return BlockUtils.IsSolid(position.x, position.y, position.z);
    }

    private void FaceTowards(Vector3 worldTarget)
    {
        Vector3 direction = worldTarget - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion desired = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desired, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
    }

    private void SnapToGround()
    {
        Vector3Int cell = CurrentCell();
        if (GridPathfinder.TryGetStandableCell(cell, out Vector3Int standable))
        {
            transform.position = CellToWorld(standable);
        }
    }

    private Vector3Int CurrentCell()
    {
        return Vector3Int.RoundToInt(transform.position);
    }

    private Vector3 CellToWorld(Vector3Int cell)
    {
        return new Vector3(cell.x, cell.y + groundYOffset, cell.z);
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Executes a dweller's physical movement over the voxel grid: following an A* path cell by
/// cell (walking, hopping up ledges, descending drops), falling when the ground is unsupported,
/// facing the direction of travel, and holding the cell reservations that stop two dwellers
/// occupying the same space.
///
/// It knows nothing about *why* it is moving. Callers (an <see cref="IDwellerTask"/> or the
/// idle behaviour) issue <see cref="FollowPath"/> / <see cref="BeginIdleHop"/> and poll
/// <see cref="IsMoving"/>, <see cref="HasArrived"/>, <see cref="IsBlocked"/> and
/// <see cref="IsFalling"/> to react.
///
/// All motion uses the simulation-scaled <see cref="Time.deltaTime"/>, so dwellers freeze while
/// the simulation is paused and speed up with the simulation speed.
/// </summary>
public class DwellerLocomotion
{
    private enum State { Idle, Moving, Hopping, Descending, Falling }

    private readonly DwellerAgent _agent;
    private readonly Transform _transform;
    private readonly DwellerLocomotionSettings _settings;

    private State _state = State.Idle;

    private List<Vector3Int> _path;
    private int _pathIndex;
    private bool _arrived;
    private bool _blocked;

    private float _stuckTimer;
    private Vector3 _lastPosition;

    // Hop animation state (take-off / landing world positions and elapsed time).
    private Vector3 _hopFrom;
    private Vector3 _hopTo;
    private float _hopTime;
    private float _activeHopHeight;
    private float _activeHopDuration;
    private bool _hopAdvancesPath;

    // Current downward speed while in the Falling state.
    private float _fallVelocity;

    // Descend state: first move horizontally to the centre of the lower cell, then fall.
    private Vector3 _descentHorizontalTarget;
    private float _descentLandingY;

    // Cell reservations that stop two dwellers occupying the same space. The dweller always
    // holds its resting cell (_occupiedCell) and, while stepping, additionally holds the cell
    // it is walking into (_stepCell) until it arrives there.
    private Vector3Int _occupiedCell;
    private Vector3Int _stepCell;
    private bool _hasStepReservation;

    public DwellerLocomotion(DwellerAgent agent, DwellerLocomotionSettings settings)
    {
        _agent = agent;
        _transform = agent.transform;
        _settings = settings;
    }

    /// <summary>True while actively following a path (walking, hopping or descending).</summary>
    public bool IsMoving => _state == State.Moving || _state == State.Hopping || _state == State.Descending;

    /// <summary>True while airborne on a vertical drop; overrides any path following.</summary>
    public bool IsFalling => _state == State.Falling;

    /// <summary>True once the most recent path has been fully walked (or was already empty).</summary>
    public bool HasArrived => _arrived;

    /// <summary>True when the mover gave up making progress (blocked cell / stuck timeout).</summary>
    public bool IsBlocked => _blocked;

    /// <summary>Begins following the given A* path. An empty/null path counts as already arrived.</summary>
    public void FollowPath(List<Vector3Int> path)
    {
        _path = path;
        _pathIndex = 0;
        _arrived = false;
        _blocked = false;
        _stuckTimer = 0f;
        _lastPosition = _transform.position;

        if (path == null || path.Count == 0)
        {
            _arrived = true;
            _state = State.Idle;
            return;
        }

        _state = State.Moving;
    }

    /// <summary>Stops any path following and settles in place (keeps the resting cell reserved).</summary>
    public void Stop()
    {
        ReleaseStepReservation();
        _path = null;
        _pathIndex = 0;
        _stuckTimer = 0f;
        _lastPosition = _transform.position;
        _state = State.Idle;
    }

    /// <summary>Advances the dweller's movement by one frame.</summary>
    public void Tick()
    {
        // If the ground was dug out from under a settled dweller (e.g. it just mined the block
        // it was standing on), start falling before doing anything else.
        if (_state == State.Idle && !IsGrounded())
        {
            BeginFall();
        }

        switch (_state)
        {
            case State.Moving:
                TickMoving();
                break;
            case State.Hopping:
                TickHopping();
                break;
            case State.Descending:
                TickDescending();
                break;
            case State.Falling:
                TickFalling();
                break;
            case State.Idle:
                break;
        }
    }

    private void TickMoving()
    {
        if (_path == null || _pathIndex >= _path.Count)
        {
            ArriveIdle();
            return;
        }

        // Claim the next cell before entering it. If another dweller is already holding it,
        // wait here (the stuck timer will eventually report us blocked) rather than walking
        // into the same space.
        if (!TryReserveStep(_path[_pathIndex]))
        {
            DetectStuck();
            return;
        }

        Vector3 target = CellToWorld(_path[_pathIndex]);

        // A rise to the next cell is cleared with a little hop rather than a diagonal glide,
        // so the dweller visibly stops and jumps up onto the ledge.
        if (target.y - _transform.position.y > 0.5f)
        {
            BeginPathHop(target);
            return;
        }

        // A drop to a lower neighbour looks better if the dweller first walks out to the centre
        // of that lower cell at its current height, then falls straight down.
        if (_transform.position.y - target.y > 0.5f)
        {
            BeginDescent(target);
            return;
        }

        FaceTowards(target);

        _transform.position = Vector3.MoveTowards(_transform.position, target, _settings.moveSpeed * Time.deltaTime);

        if ((_transform.position - target).sqrMagnitude <= _settings.waypointTolerance * _settings.waypointTolerance)
        {
            _transform.position = target;
            CompletePathStep();
            return;
        }

        DetectStuck();
    }

    private void BeginPathHop(Vector3 target)
    {
        FaceTowards(target);
        BeginHop(_transform.position, target, _settings.hopHeight, _settings.hopDuration, true);
    }

    /// <summary>Performs a little in-place hop, facing <paramref name="faceDirection"/>.</summary>
    public void BeginIdleHop(Vector3 faceDirection, float height, float duration)
    {
        FaceTowards(_transform.position + faceDirection);
        BeginHop(_transform.position, _transform.position, height, duration, false);
    }

    private void BeginHop(Vector3 from, Vector3 to, float height, float duration, bool advancePath)
    {
        _hopFrom = from;
        _hopTo = to;
        _hopTime = 0f;
        _activeHopHeight = height;
        _activeHopDuration = duration;
        _hopAdvancesPath = advancePath;
        _state = State.Hopping;
    }

    private void TickHopping()
    {
        _hopTime += Time.deltaTime;
        float duration = Mathf.Max(0.01f, _activeHopDuration);
        float t = Mathf.Clamp01(_hopTime / duration);

        // Linear glide from take-off to landing, plus a parabolic arc on top whose apex sits
        // `hopHeight` blocks above the take-off cell (clamped so it never dips).
        Vector3 pos = Vector3.Lerp(_hopFrom, _hopTo, t);
        float rise = _hopTo.y - _hopFrom.y;
        float arc = Mathf.Max(0f, 4f * (_activeHopHeight - rise * 0.5f));
        pos.y += arc * t * (1f - t);
        _transform.position = pos;

        if ((_hopTo - _hopFrom).sqrMagnitude > 0.0001f)
        {
            FaceTowards(_hopTo);
        }

        if (t >= 1f)
        {
            _transform.position = _hopTo;
            if (_hopAdvancesPath)
            {
                CompletePathStep();
                return;
            }

            if (!IsGrounded())
            {
                BeginFall();
                return;
            }

            _state = State.Idle;
        }
    }

    private void BeginDescent(Vector3 target)
    {
        _descentHorizontalTarget = new Vector3(target.x, _transform.position.y, target.z);
        _descentLandingY = target.y;
        _fallVelocity = 0f;
        _state = State.Descending;
        FaceTowards(_descentHorizontalTarget);
    }

    private void TickDescending()
    {
        if (_path == null || _pathIndex >= _path.Count)
        {
            ArriveIdle();
            return;
        }

        Vector3 pos = _transform.position;
        Vector3 horizontalTarget = new Vector3(_descentHorizontalTarget.x, pos.y, _descentHorizontalTarget.z);
        Vector3 flatDelta = horizontalTarget - pos;
        flatDelta.y = 0f;

        if (flatDelta.sqrMagnitude > _settings.waypointTolerance * _settings.waypointTolerance)
        {
            FaceTowards(horizontalTarget);
            _transform.position = Vector3.MoveTowards(pos, horizontalTarget, _settings.moveSpeed * Time.deltaTime);
            DetectStuck();
            return;
        }

        pos.x = _descentHorizontalTarget.x;
        pos.z = _descentHorizontalTarget.z;
        _fallVelocity = Mathf.Min(_settings.maxFallSpeed, _fallVelocity + _settings.fallGravity * Time.deltaTime);
        pos.y -= _fallVelocity * Time.deltaTime;

        if (pos.y <= _descentLandingY)
        {
            pos.y = _descentLandingY;
            _transform.position = pos;
            CompletePathStep();
            return;
        }

        _transform.position = pos;
    }

    private void BeginFall()
    {
        // We are leaving on a vertical drop, so give up the cell we were stepping toward. The
        // resting cell stays reserved until we land somewhere new. Any claimed job is handed
        // back by the owning task once the coordinator sees IsFalling.
        ReleaseStepReservation();

        _path = null;
        _pathIndex = 0;
        _arrived = false;
        _fallVelocity = 0f;
        _state = State.Falling;
    }

    private void TickFalling()
    {
        Vector3Int foot = CurrentCell();
        float landingY = FindLandingY(foot);

        _fallVelocity = Mathf.Min(_settings.maxFallSpeed, _fallVelocity + _settings.fallGravity * Time.deltaTime);

        Vector3 pos = _transform.position;
        pos.y -= _fallVelocity * Time.deltaTime;

        if (pos.y <= landingY)
        {
            pos.y = landingY;
            _transform.position = pos;

            // Claim the cell we landed in and release the one we fell from.
            SetOccupied(CurrentCell());
            _state = State.Idle;
            return;
        }

        _transform.position = pos;
    }

    /// <summary>
    /// World Y the feet should come to rest at: one block above the first solid block below the
    /// given cell, falling back to the world floor if the column is empty.
    /// </summary>
    private float FindLandingY(Vector3Int foot)
    {
        int minY = World.Instance.minElevation;
        for (int y = foot.y - 1; y > minY; y--)
        {
            if (BlockUtils.IsSolid(foot.x, y, foot.z))
            {
                return (y + 1) + _settings.groundYOffset;
            }
        }
        return (minY + 1) + _settings.groundYOffset;
    }

    /// <summary>True while a solid block sits directly beneath the dweller's standing cell.</summary>
    public bool IsGrounded()
    {
        Vector3Int foot = CurrentCell();
        return BlockUtils.IsSolid(foot.x, foot.y - 1, foot.z);
    }

    private void CompletePathStep()
    {
        // We have arrived at the cell we reserved as our step target; make it our new resting
        // cell and release the one we came from.
        SetOccupied(_path[_pathIndex]);

        _pathIndex++;
        _lastPosition = _transform.position;
        _stuckTimer = 0f;

        // The ground may have been dug out from under us mid-step; fall now that we are settled
        // on an integer cell where the check is reliable.
        if (!IsGrounded())
        {
            BeginFall();
            return;
        }

        if (_pathIndex >= _path.Count)
        {
            ArriveIdle();
            return;
        }

        _state = State.Moving;
    }

    private void ArriveIdle()
    {
        _arrived = true;
        _state = State.Idle;
    }

    private void DetectStuck()
    {
        if ((_transform.position - _lastPosition).sqrMagnitude > 0.0001f)
        {
            _stuckTimer = 0f;
            _lastPosition = _transform.position;
            return;
        }

        _stuckTimer += Time.deltaTime;
        if (_stuckTimer >= _settings.stuckTimeout)
        {
            // Give up making progress: report ourselves blocked and settle. The owning task
            // turns this into a failure (return the job, blacklist it briefly).
            _blocked = true;
            Stop();
        }
    }

    public void FaceTowards(Vector3 worldTarget)
    {
        Vector3 direction = worldTarget - _transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion desired = Quaternion.LookRotation(direction.normalized, Vector3.up);
        _transform.rotation = Quaternion.Slerp(_transform.rotation, desired, 1f - Mathf.Exp(-_settings.turnSpeed * Time.deltaTime));
    }

    /// <summary>Snaps the dweller onto the nearest standable cell in its column.</summary>
    public void SnapToGround()
    {
        Vector3Int cell = CurrentCell();
        if (GridPathfinder.TryGetStandableCell(cell, out Vector3Int standable))
        {
            _transform.position = CellToWorld(standable);
        }
    }

    /// <summary>Reserves the dweller's current cell as its resting cell (call once on spawn).</summary>
    public void ClaimCurrentCell()
    {
        SetOccupied(CurrentCell());
        _lastPosition = _transform.position;
    }

    /// <summary>
    /// Attempts to reserve <paramref name="cell"/> as the target of the current step so no other
    /// dweller can enter it. Returns false if another dweller already holds it.
    /// </summary>
    private bool TryReserveStep(Vector3Int cell)
    {
        if (_hasStepReservation && _stepCell == cell) return true;
        if (!DwellerOccupancy.TryReserve(cell, _agent)) return false;

        _stepCell = cell;
        _hasStepReservation = true;
        return true;
    }

    /// <summary>
    /// Makes <paramref name="cell"/> the dweller's resting cell: reserves it (if not already held
    /// from the step reservation) and releases the cell it used to occupy.
    /// </summary>
    private void SetOccupied(Vector3Int cell)
    {
        DwellerOccupancy.TryReserve(cell, _agent);
        if (_occupiedCell != cell)
        {
            DwellerOccupancy.Release(_occupiedCell, _agent);
        }

        _occupiedCell = cell;
        _stepCell = cell;
        _hasStepReservation = false;
    }

    /// <summary>Gives back the current step-target cell (if any) without touching the resting cell.</summary>
    private void ReleaseStepReservation()
    {
        if (_hasStepReservation && _stepCell != _occupiedCell)
        {
            DwellerOccupancy.Release(_stepCell, _agent);
        }

        _hasStepReservation = false;
        _stepCell = _occupiedCell;
    }

    /// <summary>Releases every cell this dweller holds, e.g. when it is disabled or destroyed.</summary>
    public void ReleaseAllReservations()
    {
        if (_hasStepReservation)
        {
            DwellerOccupancy.Release(_stepCell, _agent);
        }

        DwellerOccupancy.Release(_occupiedCell, _agent);
        _hasStepReservation = false;
    }

    public Vector3Int CurrentCell()
    {
        // Undo groundYOffset on Y so the returned cell matches the standable cell the dweller
        // actually occupies (CellToWorld adds the same offset).
        return new Vector3Int(
            Mathf.RoundToInt(_transform.position.x),
            Mathf.RoundToInt(_transform.position.y - _settings.groundYOffset),
            Mathf.RoundToInt(_transform.position.z));
    }

    private Vector3 CellToWorld(Vector3Int cell)
    {
        return new Vector3(cell.x, cell.y + _settings.groundYOffset, cell.z);
    }
}

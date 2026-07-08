using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A* pathfinding over the voxel grid for grounded dwellers.
///
/// A dweller occupies a "standable" cell: a cell whose foot- and head-space are
/// empty (air) and whose block directly below is a solid, walkable floor. Movement
/// is horizontal (N/S/E/W) with an optional one-block step up or step down, which is
/// enough for a dweller to walk across the surface and in/out of shallow dug areas.
///
/// Solidity is read from <see cref="BlockUtils.IsSolid"/> (which reflects the real
/// block data regardless of the currently visible slice), and per-block traversal
/// weight comes from <see cref="BlockDefinition.pathfindingCost"/>.
/// </summary>
public static class GridPathfinder
{
    // Horizontal neighbour offsets (no diagonals).
    private static readonly Vector3Int[] HorizontalDirections =
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
    };

    // Vertical deltas allowed while stepping to a horizontal neighbour: same level,
    // one block up, or one block down.
    private static readonly int[] VerticalSteps = { 0, 1, -1 };

    private static BlockAccessor _accessor;

    private static BlockAccessor Accessor
    {
        get
        {
            if (_accessor == null && World.Instance != null)
            {
                _accessor = new BlockAccessor(World.Instance);
            }
            return _accessor;
        }
    }

    /// <summary>
    /// True when a dweller can stand in the given cell: the cell and the cell above
    /// it are non-solid (room for a two-block-tall dweller) and the block directly
    /// below is a solid, walkable floor.
    /// </summary>
    public static bool IsStandable(Vector3Int cell)
    {
        if (World.Instance == null) return false;

        // Need a floor below to stand on.
        if (cell.y <= World.Instance.minElevation) return false;
        if (cell.y >= World.Instance.maxY) return false;

        // Foot and head space must be empty.
        if (BlockUtils.IsSolid(cell.x, cell.y, cell.z)) return false;
        if (BlockUtils.IsSolid(cell.x, cell.y + 1, cell.z)) return false;

        // Floor below must be solid and walkable.
        if (!BlockUtils.IsSolid(cell.x, cell.y - 1, cell.z)) return false;

        BlockDefinition floor = GetDefinition(new Vector3Int(cell.x, cell.y - 1, cell.z));
        return floor == null || floor.isWalkable;
    }

    /// <summary>
    /// Snaps an approximate cell to the nearest standable cell in the same column by
    /// searching a little way up and then downwards, so a spawned or displaced dweller
    /// can be grounded reliably.
    /// </summary>
    public static bool TryGetStandableCell(Vector3Int approx, out Vector3Int standable)
    {
        if (World.Instance != null)
        {
            int top = Mathf.Min(approx.y + 4, World.Instance.maxY - 1);
            for (int y = top; y > World.Instance.minElevation; y--)
            {
                Vector3Int cell = new Vector3Int(approx.x, y, approx.z);
                if (IsStandable(cell))
                {
                    standable = cell;
                    return true;
                }
            }
        }

        standable = approx;
        return false;
    }

    /// <summary>
    /// Returns the set of standable cells from which a dweller could mine the target
    /// block: the horizontal neighbours (at the target's level and one above/below)
    /// plus the cell directly on top of the target.
    /// </summary>
    public static List<Vector3Int> GetMiningStandCells(Vector3Int target)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        void TryAdd(Vector3Int c)
        {
            if (!cells.Contains(c) && IsStandable(c))
            {
                cells.Add(c);
            }
        }

        foreach (Vector3Int dir in HorizontalDirections)
        {
            TryAdd(new Vector3Int(target.x + dir.x, target.y, target.z + dir.z));
            TryAdd(new Vector3Int(target.x + dir.x, target.y + 1, target.z + dir.z));
            TryAdd(new Vector3Int(target.x + dir.x, target.y - 1, target.z + dir.z));
        }

        // Standing directly on top of the block (mining it from above).
        TryAdd(new Vector3Int(target.x, target.y + 1, target.z));
        
        // Standing directly below the block (mining it from below) Dwellers are 2 blocks tall.
        TryAdd(new Vector3Int(target.x, target.y - 2, target.z));

        return cells;
    }

    /// <summary>
    /// Finds a walking path from <paramref name="start"/> to the nearest of the given
    /// goal cells using A*. Returns the list of cells to walk through (including the
    /// reached goal, excluding the start) or null if no goal is reachable within the
    /// node budget.
    ///
    /// Cells in <paramref name="blocked"/> (e.g. those reserved by other dwellers) are
    /// treated as impassable so the route steers around them. The start cell is never
    /// blocked; a goal that is blocked simply becomes unreachable.
    /// </summary>
    public static List<Vector3Int> FindPath(Vector3Int start, ICollection<Vector3Int> goals, int maxNodes = 4000,
        HashSet<Vector3Int> blocked = null)
    {
        if (goals == null || goals.Count == 0) return null;

        HashSet<Vector3Int> goalSet = new HashSet<Vector3Int>(goals);
        if (goalSet.Contains(start))
        {
            return new List<Vector3Int>(); // Already at a goal.
        }

        var openSet = new MinHeap();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, float> { [start] = 0f };

        openSet.Push(start, Heuristic(start, goalSet));

        int expanded = 0;
        while (openSet.Count > 0 && expanded < maxNodes)
        {
            Vector3Int current = openSet.Pop();
            expanded++;

            if (goalSet.Contains(current))
            {
                return Reconstruct(cameFrom, current);
            }

            float currentG = gScore.TryGetValue(current, out float g) ? g : float.PositiveInfinity;

            foreach (Vector3Int neighbour in GetNeighbours(current, blocked))
            {
                float tentative = currentG + StepCost(neighbour);
                float known = gScore.TryGetValue(neighbour, out float ng) ? ng : float.PositiveInfinity;
                if (tentative < known)
                {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentative;
                    openSet.Push(neighbour, tentative + Heuristic(neighbour, goalSet));
                }
            }
        }

        return null;
    }

    private static IEnumerable<Vector3Int> GetNeighbours(Vector3Int cell, HashSet<Vector3Int> blocked)
    {
        foreach (Vector3Int dir in HorizontalDirections)
        {
            foreach (int dy in VerticalSteps)
            {
                Vector3Int neighbour = new Vector3Int(cell.x + dir.x, cell.y + dy, cell.z + dir.z);

                if (!IsStandable(neighbour)) continue;

                // A cell reserved by another dweller is treated as an obstacle so routes
                // steer around it rather than trying to share it.
                if (blocked != null && blocked.Contains(neighbour)) continue;

                // Stepping up requires clearance above the current cell so the dweller
                // does not clip through the block it is climbing past.
                if (dy > 0 && BlockUtils.IsSolid(cell.x, cell.y + 2, cell.z)) continue;

                yield return neighbour;
            }
        }
    }

    private static float StepCost(Vector3Int cell)
    {
        BlockDefinition floor = GetDefinition(new Vector3Int(cell.x, cell.y - 1, cell.z));
        float cost = floor != null ? Mathf.Max(1, floor.pathfindingCost) : 1f;
        return cost;
    }

    private static float Heuristic(Vector3Int cell, HashSet<Vector3Int> goals)
    {
        float best = float.PositiveInfinity;
        foreach (Vector3Int goal in goals)
        {
            float h = Mathf.Abs(cell.x - goal.x) + Mathf.Abs(cell.y - goal.y) + Mathf.Abs(cell.z - goal.z);
            if (h < best) best = h;
        }
        return best;
    }

    private static List<Vector3Int> Reconstruct(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        List<Vector3Int> path = new List<Vector3Int> { current };
        while (cameFrom.TryGetValue(current, out Vector3Int previous))
        {
            current = previous;
            path.Add(current);
        }
        path.Reverse();
        // Drop the start cell; the caller only needs the cells to move through.
        if (path.Count > 0) path.RemoveAt(0);
        return path;
    }

    private static BlockDefinition GetDefinition(Vector3Int cell)
    {
        BlockData data = Accessor?.GetBlockDataFromPosition(cell);
        return data?.definition;
    }

    /// <summary>
    /// Minimal binary min-heap keyed by f-score, sufficient for grid A* without the
    /// allocation churn of re-sorting a list every push.
    /// </summary>
    private class MinHeap
    {
        private readonly List<Vector3Int> _items = new List<Vector3Int>();
        private readonly List<float> _priorities = new List<float>();

        public int Count => _items.Count;

        public void Push(Vector3Int item, float priority)
        {
            _items.Add(item);
            _priorities.Add(priority);
            int i = _items.Count - 1;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_priorities[parent] <= _priorities[i]) break;
                Swap(i, parent);
                i = parent;
            }
        }

        public Vector3Int Pop()
        {
            Vector3Int result = _items[0];
            int last = _items.Count - 1;
            _items[0] = _items[last];
            _priorities[0] = _priorities[last];
            _items.RemoveAt(last);
            _priorities.RemoveAt(last);

            int i = 0;
            int count = _items.Count;
            while (true)
            {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;
                if (left < count && _priorities[left] < _priorities[smallest]) smallest = left;
                if (right < count && _priorities[right] < _priorities[smallest]) smallest = right;
                if (smallest == i) break;
                Swap(i, smallest);
                i = smallest;
            }

            return result;
        }

        private void Swap(int a, int b)
        {
            (_items[a], _items[b]) = (_items[b], _items[a]);
            (_priorities[a], _priorities[b]) = (_priorities[b], _priorities[a]);
        }
    }
}

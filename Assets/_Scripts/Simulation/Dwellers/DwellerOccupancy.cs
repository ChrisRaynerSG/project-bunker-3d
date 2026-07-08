using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which grid cell each dweller occupies so that no two dwellers ever stand in,
/// or walk into, the same cell (which would otherwise let their models clip through one
/// another).
///
/// A dweller reserves its resting cell and, while stepping, additionally reserves the
/// cell it is moving into; a reservation is only released once the dweller has actually
/// left the cell. Pathfinding queries the reservations (see the <c>blocked</c> argument
/// on <see cref="GridPathfinder.FindPath"/>) so dwellers route around cells other
/// dwellers are holding rather than piling into them.
///
/// Only the foot cell is tracked: a cell is only "standable" when the block below it is
/// solid, so a dweller can never stand one block above another's feet. Distinct foot
/// cells therefore guarantee the two-block-tall bodies never overlap.
/// </summary>
public static class DwellerOccupancy
{
    private static readonly Dictionary<Vector3Int, DwellerAgent> Reservations =
        new Dictionary<Vector3Int, DwellerAgent>();

    /// <summary>
    /// Reserves <paramref name="cell"/> for <paramref name="agent"/>. Returns true if the
    /// cell was free (or already owned by this agent); false if another dweller holds it.
    /// </summary>
    public static bool TryReserve(Vector3Int cell, DwellerAgent agent)
    {
        if (Reservations.TryGetValue(cell, out DwellerAgent owner))
        {
            return owner == agent;
        }

        Reservations[cell] = agent;
        return true;
    }

    /// <summary>Releases <paramref name="cell"/>, but only if <paramref name="agent"/> owns it.</summary>
    public static void Release(Vector3Int cell, DwellerAgent agent)
    {
        if (Reservations.TryGetValue(cell, out DwellerAgent owner) && owner == agent)
        {
            Reservations.Remove(cell);
        }
    }

    /// <summary>True when a dweller other than <paramref name="agent"/> is holding the cell.</summary>
    public static bool IsBlocked(Vector3Int cell, DwellerAgent agent)
    {
        return Reservations.TryGetValue(cell, out DwellerAgent owner) && owner != agent;
    }

    /// <summary>
    /// Builds the set of cells reserved by dwellers other than <paramref name="self"/>, for
    /// use as pathfinding obstacles so a dweller plans a route that avoids the others.
    /// </summary>
    public static HashSet<Vector3Int> GetBlockedCells(DwellerAgent self)
    {
        HashSet<Vector3Int> blocked = new HashSet<Vector3Int>();
        foreach (KeyValuePair<Vector3Int, DwellerAgent> reservation in Reservations)
        {
            if (reservation.Value != self)
            {
                blocked.Add(reservation.Key);
            }
        }
        return blocked;
    }
}

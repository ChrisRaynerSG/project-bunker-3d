using UnityEngine;

/// <summary>
/// Utility methods for surface detection and terrain analysis in the voxel world.
/// </summary>
public static class SurfaceUtils
{
    /// <summary>
    /// Finds the Y coordinate of the first solid (non-air) block below the given starting position.
    /// </summary>
    /// <param name="startPosition">The starting position to search from (typically above the terrain).</param>
    /// <returns>
    /// The Y coordinate of the surface block (the first non-air block below the starting position).
    /// If no solid block is found, returns the lowest Y checked.
    /// </returns>
    public static int FindSurfaceY(Vector3Int startPosition)
    {
        BlockAccessor accessor = new BlockAccessor(World.Instance);

        BlockDefinition airBlock = BlockDatabase.Instance.GetBlockDefinition("bunker:air_block");
        int y = startPosition.y;
        while (y > 1)
        {
            var blockBelow = accessor.GetBlockDataFromPosition(new Vector3Int(startPosition.x, y - 1, startPosition.z));
            if (blockBelow == null || blockBelow.definition == null || blockBelow.definition != airBlock)
            {
                break; // Found a non-air block below, stop searching
            }
            y--;
        }
        return y;
    }
}
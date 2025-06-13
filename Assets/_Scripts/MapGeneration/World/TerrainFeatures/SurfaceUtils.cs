using System.Numerics;
using UnityEngine;

public static class SurfaceUtils
{
    public static int FindSurfaceY(Vector3Int startPosition)
    {
        BlockAccessor accessor = new BlockAccessor(World.Instance);
        Debug.Log($"Accessor: {accessor}");


        BlockDefinition airBlock = BlockDatabase.Instance.GetBlock("bunker:air_block");
        int y = startPosition.y;
        while (y > 1)
        {
            var blockBelow = accessor.GetBlock(new Vector3Int(startPosition.x, y - 1, startPosition.z));
            if (blockBelow == null || blockBelow.definition == null || blockBelow.definition != airBlock)
            {
                break; // Found a non-air block below, stop searching
            }
            y--;
        }
        return y;
    }
}
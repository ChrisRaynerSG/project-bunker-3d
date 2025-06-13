using Unity.Collections;
using UnityEngine;

public static class TreeUtilities
{
    public static BlockAccessor blockAccessor;
    public static void GenerateTree(Vector3 position)
    {
        int trunkHeight = (int)position.y + 5;

        for (int y = (int)position.y; y < trunkHeight; y++)
        {
            // Create trunk blocks
            blockAccessor.SetBlock(new Vector3Int((int)position.x, y, (int)position.z),
                BlockDatabase.Instance.GetBlock("bunker:oak_tree_log_block"));
        }

        for (int dy = -2; dy <= 2; dy++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dz = -2; dz <= 2; dz++)
                {
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) + Mathf.Abs(dz) <= 3) // Sphere-like shape
                    {
                        int lx = (int)position.x + dx;
                        int ly = (int)position.y + trunkHeight + dy;
                        int lz = (int)position.z + dz;

                    
                        // need to get the block info from WorldData from y slice to get chunk to get block info
                        blockAccessor.SetBlock(new Vector3Int(lx, ly, lz),
                            BlockDatabase.Instance.GetBlock("bunker:oak_tree_leaves_block"));
                    }
                }
            }
        }

    }
}
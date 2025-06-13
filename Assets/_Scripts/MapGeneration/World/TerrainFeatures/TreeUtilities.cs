using Unity.Collections;
using UnityEngine;

public static class TreeUtilities
{
    public static BlockAccessor blockAccessor;
    public static void GenerateTree(Vector3 position, float radius = 5f, System.Random rng = null)
    {
        blockAccessor = new BlockAccessor(World.Instance);

        if (IsTreeNearby(position, radius))
        {
            return; // Don't generate a tree if there's already one nearby
        }
        if (IsTreeNearWorldEdge(position, radius))
        {
            return; // Don't generate a tree near the world edge
        }

        Debug.Log($"Generating tree at position: {position}");

        rng ??= Randomizer.GetDeterministicRNG(position, World.Instance.seed);

        int trunkHeight = (int)position.y + rng.Next(4, 9);

        for (int y = (int)position.y; y < trunkHeight; y++)
        {
            BlockData block = blockAccessor.GetBlock(new Vector3Int((int)position.x, y, (int)position.z));
            // Create trunk blocks
            Debug.Log($"Setting trunk block at position: {new Vector3Int((int)position.x, y, (int)position.z)}");
            blockAccessor.SetBlockNoMeshUpdate(new Vector3Int((int)position.x, y, (int)position.z),
                BlockDatabase.Instance.GetBlock("bunker:oak_tree_log_block"));

            block.IsSolid = true; // Set the block as solid

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
                        int ly = trunkHeight + dy;
                        int lz = (int)position.z + dz;
                        // need to get the block info from WorldData from y slice to get chunk to get block info

                        BlockData block = blockAccessor.GetBlock(new Vector3Int(lx, ly, lz));
                        if (block == null || block.definition == null)
                        {
                            Debug.LogWarning($"Block at {new Vector3Int(lx, ly, lz)} is null or has no definition.");
                        }

                        if (block.definition.id == BlockDatabase.Instance.GetBlock("bunker:oak_tree_log_block").id)
                        {
                            // If the block is already a log, skip setting leaves
                            continue;
                        }
                        // Only set leaves if the block is not already a log
                        blockAccessor.SetBlockNoMeshUpdate(new Vector3Int(lx, ly, lz),
                        BlockDatabase.Instance.GetBlock("bunker:oak_tree_leaves_block"));
                        block.IsSolid = true;
                    }
                }
            }
        }
    }

    public static bool IsTreeNearby(Vector3 position, float radius)
    {
        blockAccessor = new BlockAccessor(World.Instance);
        BlockDefinition targetBlock = BlockDatabase.Instance.GetBlock("bunker:oak_tree_log_block");

        for (int x = (int)(position.x - radius); x <= (int)(position.x + radius); x++)
        {
            for (int z = (int)(position.z - radius); z <= (int)(position.z + radius); z++)
            {
                for (int y = (int)(position.y - radius); y <= (int)(position.y + radius); y++)
                {
                    BlockData block = blockAccessor.GetBlock(new Vector3Int(x, y, z));
                    if (block != null && block.definition != null &&
                        block.definition.id == targetBlock.id)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static bool IsTreeNearWorldEdge(Vector3 position, float radius)
    {
        // need to make sure that the position is further than 4 blocks from the edge of the world
        int worldSize = World.Instance.maxX; // Assuming World.Instance.WorldSize gives the size of the world
        return position.x < radius || position.x > worldSize - radius || position.z < radius || position.z > worldSize - radius;
    }

    public static void GenerateNaturalHedges(int count = 30, int maxLength = 40)
    {
        BlockAccessor blockAccessor = new BlockAccessor(World.Instance);
        BlockDefinition leavesBlock = BlockDatabase.Instance.GetBlock("bunker:oak_tree_leaves_block");
        BlockDefinition airBlock = BlockDatabase.Instance.GetBlock("bunker:air_block");

        System.Random rng = new System.Random(World.Instance.seed);
        int worldSize = World.Instance.maxX; // Assuming World.Instance.WorldSize gives the size of the world
        for (int i = 0; i < count; i++)
        {
            int x = rng.Next(4, worldSize - 4);
            int z = rng.Next(4, worldSize - 4);
            int dirX = rng.Next(-1, 2); // -1, 0, or 1
            int dirZ = rng.Next(-1, 2); // -1, 0, or 1
            if (dirX == 0 && dirZ == 0)
            {
                dirX = 1;
            }
            int length = rng.Next(10, maxLength);
            for (int j = 0; j < length; j++)
            {
                if (x < 0 || x >= worldSize || z < 0 || z >= worldSize)
                {
                    break; // Out of bounds, stop generating
                }
                int y = SurfaceUtils.FindSurfaceY(new Vector3Int(x, World.Instance.maxY - World.Instance.minElevation - 1, z));
                if (y <= 0)
                {
                    continue; // Skip if the surface is below ground level
                }
                int height = rng.Next(1, 3);
                for (int h = 0; h < height; h++)
                {
                    Vector3Int position = new Vector3Int(x, y + h, z);
                    blockAccessor.SetBlockNoMeshUpdate(position, leavesBlock);
                    BlockData block = blockAccessor.GetBlock(position);
                    if(block == null)
                    {
                        Debug.LogWarning($"Block at {position} is null.");
                        continue; // Skip if the block is null
                    }
                    block.IsSolid = true;
                }

                if (rng.NextDouble() < 0.3f)
                {
                    int turn = rng.Next(3) - 1;
                    if (rng.NextDouble() < 0.5f)
                    {
                        dirX = Mathf.Clamp(dirX + turn, -1, 1);
                    }
                    else
                    {
                        dirZ = Mathf.Clamp(dirZ + turn, -1, 1);
                    }
                    if (dirX == 0 && dirZ == 0)
                    {
                        dirX = 1; // Ensure we always have a direction
                    }
                }
                x += dirX;
                z += dirZ;
            }
        }
    }
}
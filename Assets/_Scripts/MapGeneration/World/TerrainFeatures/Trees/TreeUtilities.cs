
using UnityEngine;

/// <summary>
/// Provides utility methods for procedural generation of trees and natural hedges in the world.
/// 
/// <see cref="TreeUtilities"/> includes logic for placing trees with collision and edge checks,
/// as well as generating random hedges for natural scenery. All methods operate directly on the world's block data.
/// </summary>
public static class TreeUtilities
{
    /// <summary>
    /// The block accessor used for block manipulation during tree and hedge generation.
    /// </summary>
    public static BlockAccessor blockAccessor;

    /// <summary>
    /// Generates a tree at the specified position, with optional radius and random seed.
    /// Ensures no other tree is nearby and the tree is not too close to the world edge.
    /// </summary>
    /// <param name="position">The world position to generate the tree.</param>
    /// <param name="radius">The minimum distance from other trees and the world edge.</param>
    /// <param name="rng">Optional random number generator for deterministic placement.</param>
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

        rng ??= Randomizer.GetDeterministicRNG(position, World.Instance.seed);

        TreeGameData tree = new TreeGameData();

        int trunkHeight = (int)position.y + rng.Next(4, 9);

        // Generate trunk
        for (int y = (int)position.y; y < trunkHeight; y++)
        {
            Vector3Int trunkPosition = new Vector3Int((int)position.x, y, (int)position.z);
            blockAccessor.SetBlockNoMeshUpdate(trunkPosition, BlockDatabase.Instance.GetBlockDefinition("bunker:oak_tree_log_block"));
            tree.AddLogBlock(blockAccessor.GetBlockDataFromPosition(trunkPosition));
            tree.LogPositions.Add(trunkPosition);
        }

        // Generate leaves in a sphere-like shape
        for (int dy = -2; dy <= 2; dy++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dz = -2; dz <= 2; dz++)
                {
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) + Mathf.Abs(dz) <= 3)
                    {
                        int lx = (int)position.x + dx;
                        int ly = trunkHeight + dy;
                        int lz = (int)position.z + dz;

                        BlockData block = blockAccessor.GetBlockDataFromPosition(new Vector3Int(lx, ly, lz));
                        if (block == null || block.definition == null)
                        {
                            Debug.LogWarning($"Block at {new Vector3Int(lx, ly, lz)} is null or has no definition.");
                        }

                        if (block.definition.id == BlockDatabase.Instance.GetBlockDefinition("bunker:oak_tree_log_block").id)
                        {
                            continue; // Skip setting leaves if already a log
                        }
                        blockAccessor.SetBlockNoMeshUpdate(new Vector3Int(lx, ly, lz), BlockDatabase.Instance.GetBlockDefinition("bunker:oak_tree_leaves_block"));
                        tree.AddLeafBlock(blockAccessor.GetBlockDataFromPosition(lx, ly, lz));
                        tree.LeafPositions.Add(new Vector3Int(lx, ly, lz));
                    }
                }
            }
        }
        WorldData.Instance.AddTree(tree);
    }

    /// <summary>
    /// Checks if there is a tree log block within the given radius of the specified position.
    /// </summary>
    /// <param name="position">The position to check around.</param>
    /// <param name="radius">The radius to search for existing trees.</param>
    /// <returns>True if a tree is nearby, false otherwise.</returns>
    public static bool IsTreeNearby(Vector3 position, float radius)
    {
        blockAccessor = new BlockAccessor(World.Instance);
        BlockDefinition targetBlock = BlockDatabase.Instance.GetBlockDefinition("bunker:oak_tree_log_block");

        for (int x = (int)(position.x - radius); x <= (int)(position.x + radius); x++)
        {
            for (int z = (int)(position.z - radius); z <= (int)(position.z + radius); z++)
            {
                for (int y = (int)(position.y - radius); y <= (int)(position.y + radius); y++)
                {
                    BlockData block = blockAccessor.GetBlockDataFromPosition(new Vector3Int(x, y, z));
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

    /// <summary>
    /// Checks if the specified position is too close to the world edge to generate a tree.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <param name="radius">The minimum distance from the world edge.</param>
    /// <returns>True if the position is near the world edge, false otherwise.</returns>
    public static bool IsTreeNearWorldEdge(Vector3 position, float radius)
    {
        int worldSize = World.Instance.maxX;
        return position.x < radius || position.x > worldSize - radius || position.z < radius || position.z > worldSize - radius;
    }

    /// <summary>
    /// Generates a number of natural-looking hedges in the world using random walks.
    /// </summary>
    /// <param name="count">The number of hedges to generate.</param>
    /// <param name="maxLength">The maximum length of each hedge.</param>
    public static void GenerateNaturalHedges(int count = 30, int maxLength = 40)
    {
        BlockAccessor blockAccessor = new BlockAccessor(World.Instance);
        BlockDefinition leavesBlock = BlockDatabase.Instance.GetBlockDefinition("bunker:oak_tree_leaves_block");

        System.Random rng = new System.Random(World.Instance.seed);
        int worldSize = World.Instance.maxX;
        for (int i = 0; i < count; i++)
        {
            int x = rng.Next(4, worldSize - 4);
            int z = rng.Next(4, worldSize - 4);
            int dirX = rng.Next(-1, 2);
            int dirZ = rng.Next(-1, 2);
            if (dirX == 0 && dirZ == 0)
            {
                dirX = 1;
            }
            int length = rng.Next(10, maxLength);
            for (int j = 0; j < length; j++)
            {
                if (x < 0 || x >= worldSize || z < 0 || z >= worldSize)
                {
                    break;
                }
                int y = SurfaceUtils.FindSurfaceY(new Vector3Int(x, World.Instance.maxY, z));
                if (y <= 0)
                {
                    continue;
                }
                int height = rng.Next(1, 3);
                for (int h = 0; h < height; h++)
                {
                    Vector3Int position = new Vector3Int(x, y + h, z);
                    blockAccessor.SetBlockNoMeshUpdate(position, leavesBlock);
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
                        dirX = 1;
                    }
                }
                x += dirX;
                z += dirZ;
            }
        }
    }

    /// <summary>
    ///  Finds a tree at the specified position by checking its log positions.
    /// </summary>
    /// <param name="position">The position to check for a tree.</param>
    /// <returns>The TreeGameData if a tree is found at the position, otherwise null.</returns>
    public static TreeGameData FindTreeAtPosition(Vector3Int position)
    {
        foreach (var t in WorldData.Instance.Trees)
        {
            foreach (var logPos in t.LogPositions)
            {
                if (logPos == position)
                    return t;
            }
        }
        return null;
    }

}
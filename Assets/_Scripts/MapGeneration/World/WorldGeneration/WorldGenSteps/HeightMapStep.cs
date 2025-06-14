using UnityEngine;

/// <summary>
/// HeightMapStep is a modular world generation step that creates the terrain heightmap
/// and assigns block types (grass, dirt, stone) for each position in the world grid.
/// 
/// This step uses Perlin/simplex noise (via FastNoise) to determine the surface height at each (x, z) coordinate,
/// then fills all blocks below that height as solid. The topmost block is assigned as grass or dirt
/// based on a secondary noise function (for tree placement), the next few layers as dirt, and the rest as stone.
/// 
/// Dependencies such as the block database and block accessor are provided via the WorldGenContext,
/// making this step reusable and easy to test.
/// </summary>
public class HeightMapStep : IWorldGenStep
{
    /// <summary>
    /// Applies the heightmap generation to the provided world data using the given context.
    /// The context must provide a block database and block accessor.
    /// </summary>
    /// <param name="data">The world data structure to modify.</param>
    /// <param name="context">The context containing world generation parameters and dependencies.</param>
    public void Apply(WorldData data, WorldGenContext context)
    {
        var noise = NoiseProvider.CreateNoise(context.frequency, context.seed);
        var treeNoise = NoiseProvider.CreateNoise(context.frequency * 4f, (int)(context.seed / 2f) + 4);

        int[,] heights = GenerateHeights(context, noise);

        FillBlocks(data, context, heights, treeNoise);
    }

    /// <summary>
    /// Generates a 2D heightmap array using noise for the given world context.
    ///  This heightmap determines the surface height at each (x, z) coordinate,
    ///  And then adds to the context for further processing.
    /// </summary>
    private int[,] GenerateHeights(WorldGenContext context, FastNoise noise)
    {
        int[,] heights = new int[context.maxX, context.maxZ];
        for (int x = 0; x < context.maxX; x++)
        {
            for (int z = 0; z < context.maxZ; z++)
            {
                float rawHeight = noise.GetNoise(x, z);
                int height = Mathf.FloorToInt((rawHeight + 1) * 0.5f * context.maxTerrainHeight);
                heights[x, z] = height;
            }
        }
        context.heights = heights;
        return heights;
    }

    /// <summary>
    /// Fills the world data blocks up to the heightmap, assigning block types for surface, subsurface, and base layers.
    /// </summary>
    private void FillBlocks(
        WorldData data, WorldGenContext context, int[,] heights, FastNoise treeNoise)
    {
        for (int x = 0; x < context.maxX; x++)
        {
            for (int z = 0; z < context.maxZ; z++)
            {
                for (int y = context.minElevation; y < context.maxY; y++)
                {
                    if (y < heights[x, z])
                    {
                        SetBlockDefinition(context, treeNoise, x, y, z, heights[x, z]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Determines and sets the block type (grass, dirt, stone) for a given position based on height and noise,
    /// using the block accessor and block database from the context.
    /// </summary>
    private void SetBlockDefinition(
        WorldGenContext context, FastNoise treeNoise, int x, int y, int z, int surfaceHeight)
    {
        if (y == surfaceHeight - 1)
        {
            context.blockAccessor.SetBlockNoMeshUpdate(
                new Vector3Int(x, y, z),
                treeNoise.GetNoise(x, y, z) > 0.4f
                    ? context.blockDatabase.GetBlockDefinition("bunker:dirt_block")
                    : context.blockDatabase.GetBlockDefinition("bunker:grass_block")
            );
        }
        else if (y < surfaceHeight - 1 && y > surfaceHeight - 8)
        {
            context.blockAccessor.SetBlockNoMeshUpdate(
                new Vector3Int(x, y, z),
                context.blockDatabase.GetBlockDefinition("bunker:dirt_block")
            );
        }
        else
        {
            context.blockAccessor.SetBlockNoMeshUpdate(
                new Vector3Int(x, y, z),
                context.blockDatabase.GetBlockDefinition("bunker:stone_block")
            );
        }
    }
}
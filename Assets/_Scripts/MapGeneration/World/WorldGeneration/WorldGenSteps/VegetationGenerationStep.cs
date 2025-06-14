using UnityEngine;

/// <summary>
/// VegetationGenerationStep is a world generation step that places vegetation, such as trees, on the terrain surface.
/// 
/// This step uses noise functions to determine suitable locations for vegetation and tree density. For each surface block,
/// it may convert the block to dirt and, based on a secondary noise threshold, generate a tree at that position.
/// The density and distribution of vegetation can be controlled by adjusting the noise frequency and thresholds.
/// </summary>
public class VegetationGenerationStep : IWorldGenStep
{
    /// <summary>
    /// Applies vegetation generation to the provided world data using the given context.
    /// Uses noise to determine where to place dirt and generate trees on the terrain surface.
    /// </summary>
    /// <param name="data">The world data structure to modify.</param>
    /// <param name="context">The context containing world generation parameters and dependencies.</param>
    public void Apply(WorldData data, WorldGenContext context)
    {
        int treeSeed = Mathf.Clamp(context.seed + 3, 1, int.MaxValue);
        FastNoise treeNoise = NoiseProvider.CreateNoise(context.frequency * 4f, treeSeed / 2 + 3);
        FastNoise treeDensityNoise = NoiseProvider.CreateNoise(context.frequency * 12f, treeSeed / 2 + 3);

        for (int x = 0; x < context.maxX; x++)
        {
            for (int z = 0; z < context.maxZ; z++)
            {
                int height = context.heights[x, z];

                if (treeNoise.GetNoise(x, height, z) < 0.0001f)
                {
                    context.blockAccessor.SetBlockNoMeshUpdate(
                        new Vector3Int(x, height - 1, z),
                        context.blockDatabase.GetBlockDefinition("bunker:dirt_block")
                    );
                    if (treeDensityNoise.GetNoise(x, height, z) < 0.000005f)
                    {
                        TreeUtilities.GenerateTree(new Vector3Int(x, height, z), 5f);
                    }
                }
            }
        }
    }
}
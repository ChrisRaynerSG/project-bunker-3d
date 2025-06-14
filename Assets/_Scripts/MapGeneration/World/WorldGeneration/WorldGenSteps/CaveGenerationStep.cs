using UnityEngine;

/// <summary>
/// CaveGenerationStep is a world generation step that carves out caves in the terrain using 3D noise.
/// 
/// This step iterates through the underground portion of the world and uses a noise function to determine
/// where to replace solid blocks with air blocks, creating natural-looking cave systems. The density and
/// distribution of caves can be controlled by adjusting the noise frequency and threshold.
/// </summary>
public class CaveGenerationStep : IWorldGenStep
{
    /// <summary>
    /// Applies cave generation to the provided world data using the given context.
    /// Uses 3D noise to determine which blocks should be set to air, forming caves.
    /// </summary>
    /// <param name="data">The world data structure to modify.</param>
    /// <param name="context">The context containing world generation parameters and dependencies.</param>
    public void Apply(WorldData data, WorldGenContext context)
    {
        int caveSeed = Mathf.Clamp(context.seed + 2, 1, int.MaxValue);
        FastNoise caveNoise = NoiseProvider.CreateNoise(context.frequency * 2f, caveSeed / 2 + 2);

        for (int x = 0; x < context.maxX; x++)
        {
            for (int z = 0; z < context.maxZ; z++)
            {
                for (int y = context.minElevation + 5; y < context.heights[x, z] - 5; y++)
                {
                    float noiseValue = caveNoise.GetNoise(x, y, z);
                    if (noiseValue > 0.8f)
                    {
                        context.blockAccessor.SetBlockNoMeshUpdate(
                            new Vector3Int(x, y, z),
                            context.blockDatabase.GetBlockDefinition("bunker:air_block")
                        );
                    }
                }
            }
        }
    }
}
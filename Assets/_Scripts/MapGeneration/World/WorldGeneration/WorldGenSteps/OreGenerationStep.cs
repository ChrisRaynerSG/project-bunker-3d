using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// OreGenerationStep is a world generation step that distributes ore blocks within the terrain using 3D noise.
/// 
/// This step iterates through underground blocks below the surface and uses a noise function to determine
/// where to replace stone blocks with ore blocks (e.g., coal ore). The density and distribution of ores
/// can be controlled by adjusting the noise frequency and threshold.
/// </summary>
public class OreGenerationStep : IWorldGenStep
{
    /// <summary>
    /// Applies ore generation to the provided world data using the given context.
    /// Uses 3D noise to determine which stone blocks should be replaced with ore blocks.
    /// </summary>
    /// <param name="data">The world data structure to modify.</param>
    /// <param name="context">The context containing world generation parameters and dependencies.</param>
    public void Apply(WorldData data, WorldGenContext context)
    {
        int oreSeed = Mathf.Clamp(context.seed + 1, 1, int.MaxValue);
        FastNoise oreNoise = NoiseProvider.CreateNoise(context.frequency * 10f, oreSeed / 2 + 1);

        for (int x = 0; x < context.maxX; x++)
        {
            for (int z = 0; z < context.maxZ; z++)
            {
                if (context.heights[x, z] <= context.minElevation)
                    continue;

                for (int y = context.minElevation; y < context.heights[x, z]; y++)
                {
                    float noiseValue = oreNoise.GetNoise(x, y, z);
                    if (noiseValue > 0.5f)
                    {
                        var blockData = context.blockAccessor.GetBlockDataFromPosition(x, y, z);
                        if (blockData != null && blockData.definition.id == "bunker:stone_block")
                        {
                            context.blockAccessor.SetBlockNoMeshUpdate(
                                new Vector3Int(x, y, z),
                                context.blockDatabase.GetBlockDefinition("bunker:coal_ore_block")
                            );
                        }
                    }
                }
            }
        }
    }
}
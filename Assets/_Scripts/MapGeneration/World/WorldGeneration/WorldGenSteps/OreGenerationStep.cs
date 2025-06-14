using Unity.Mathematics;
using UnityEngine;

public class OreGenerationStep : IWorldGenStep
{
    public void Apply(WorldData data, WorldGenContext context)
    {
        int oreSeed = Mathf.Clamp(context.seed + 1, 1, int.MaxValue);
        FastNoise oreNoise = NoiseProvider.CreateNoise(context.frequency * 10f, oreSeed / 2 + 1);

        for (int x = 0; x < context.maxX; x++)
        {
            for (int z = 0; z < context.maxZ; z++)
            {
                if (context.heights[x, z] <= context.minElevation)
                    continue; // Skip if the height is below the minimum elevation
                //only generate ores below the surface
                for (int y = context.minElevation; y < context.heights[x, z]; y++)
                {
                    float noiseValue = oreNoise.GetNoise(x, y, z);
                    if (noiseValue > 0.5f) // Adjust this threshold to control ore density
                    {
                        if (context.blockAccessor.GetBlockDataFromPosition(x, y, z) != null && context.blockAccessor.GetBlockDataFromPosition(x, y, z).definition.id == "bunker:stone_block")
                        {
                            // Set coal ore block if the current block is stone
                            context.blockAccessor.SetBlockNoMeshUpdate(new Vector3Int(x, y, z), context.blockDatabase.GetBlockDefinition("bunker:coal_ore_block"));
                        }
                    }
                }
            }
        }
    }
}
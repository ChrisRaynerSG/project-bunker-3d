using System.IO.Compression;
using UnityEngine;
public class CaveGenerationStep : IWorldGenStep
{
    public void Apply(WorldData data, WorldGenContext context)
    {
        int caveSeed = Mathf.Clamp(context.seed + 2, 1, int.MaxValue);
        FastNoise caveNoise = NoiseProvider.CreateNoise(context.frequency * 7f, caveSeed / 2 + 2);
        // Cave generation logic goes here
        // This is a placeholder implementation
        for (int x = 0; x < context.maxX; x++)
        {
            for (int z = 0; z < context.maxZ; z++)
            {
                for (int y = context.minElevation; y < context.heights[x,z] - 1; y++)
                {
                    float noiseValue = caveNoise.GetNoise(x, y, z);
                    if (noiseValue < 0.05f) // Adjust this threshold to control cave density
                    {
                        context.blockAccessor.SetBlockNoMeshUpdate(new Vector3Int(x, y, z), context.blockDatabase.GetBlockDefinition("bunker:air_block"));
                    }
                }
            }
        }
    }
}
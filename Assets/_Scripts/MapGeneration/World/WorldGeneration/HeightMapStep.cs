using UnityEngine;

public class HeightMapStep : IWorldGenStep
{
    public void Apply(WorldData data, WorldGenContext context)
    {
        var noise = NoiseProvider.CreateNoise(context.frequency, context.seed);
        var treeNoise = NoiseProvider.CreateNoise(context.frequency * 4f, (int)(context.seed / 2f) + 4);

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

         for (int x = 0; x < context.maxX; x++)
        {
            for (int z = 0; z < context.maxZ; z++)
            {
                for (int y = context.minElevation; y < context.maxY; y++)
                {
                    if (y < heights[x, z])
                    {
                        int yIndex = y - context.minElevation;
                        int chunkX = x / ChunkData.CHUNK_SIZE;
                        int chunkZ = z / ChunkData.CHUNK_SIZE;
                        int chunkLocalX = x % ChunkData.CHUNK_SIZE;
                        int chunkLocalZ = z % ChunkData.CHUNK_SIZE;

                        BlockData blockData = data.YSlices[yIndex].Chunks[chunkX][chunkZ].Grid[chunkLocalX][chunkLocalZ];
                        blockData.IsSolid = true;

                        // Surface
                        if (y == heights[x, z] - 1)
                        {
                            if (treeNoise.GetNoise(x, y, z) > 0.4f)
                                blockData.definition = BlockDatabase.Instance.GetBlockDefinition("bunker:dirt_block");
                            else
                                blockData.definition = BlockDatabase.Instance.GetBlockDefinition("bunker:grass_block");
                        }
                        // Subsurface
                        else if (y < heights[x, z] - 1 && y > heights[x, z] - 8)
                        {
                            blockData.definition = BlockDatabase.Instance.GetBlockDefinition("bunker:dirt_block");
                        }
                        // Base
                        else
                        {
                            blockData.definition = BlockDatabase.Instance.GetBlockDefinition("bunker:stone_block");
                        }
                    }
                }
            }
        }
    }
}
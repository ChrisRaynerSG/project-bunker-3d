using Unity.Mathematics;
using UnityEngine;

public class BlockDecider
{
    private int _seed;
    private BlockDatabase _blockDatabase;
    private FastNoise _heightNoise;
    private FastNoise _oreNoise;

    private int maxY;

    private float frequency = 0.007f;

    private int dirtDepth = 5;

    public BlockDecider(int seed, int maxY = 64)
    {

        _seed = seed;
        _blockDatabase = BlockDatabase.Instance;

        _heightNoise = NoiseProvider.CreateNoise(frequency, seed);

        _oreNoise = NoiseProvider.CreateNoise(frequency * 10, (seed / 2) + 1);

    }

    public ushort GetBlockId(int3 worldPosition)
    {
        float rawHeight = _heightNoise.GetNoise(worldPosition.x, worldPosition.z);
        int surfaceHeight = Mathf.FloorToInt((rawHeight + 1f) * 0.5f * maxY);

        if (worldPosition.y > surfaceHeight)
        {
            return _blockDatabase.GetBlockId("bunker:air_block");
        }
        else if(worldPosition.y == surfaceHeight)
        {
            return _blockDatabase.GetBlockId("bunker:grass_block");
        }
        else if (worldPosition.y > surfaceHeight - dirtDepth)
        {
            return _blockDatabase.GetBlockId("bunker:dirt_block");
        }
        else
        {
            float oreNoiseValue = _oreNoise.GetNoise(worldPosition.x, worldPosition.y, worldPosition.z);
            if (oreNoiseValue > 0.5f)
            {
                return _blockDatabase.GetBlockId("bunker:stone_block");
            }
            else
            {
                return _blockDatabase.GetBlockId("bunker:bedrock_block");
            }
        }
    }
}
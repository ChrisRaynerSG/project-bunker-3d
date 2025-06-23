using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct BlockDecider
{
    [ReadOnly] private BlockDatabase _blockDatabase;
    private NativeArray<float> _heightMap;
    private int maxY;

    private int dirtDepth;

    public BlockDecider(NativeArray<float> heightMap, int maxY = 16)
    {
        dirtDepth = 5;
        this.maxY = maxY;
        _heightMap = heightMap; //precompute height map elsewhere to allow for burst compilation with ECS
        _blockDatabase = BlockDatabase.Instance;
    }

    public ushort GetBlockId(int3 worldPosition)
    {
        (int x, int y, int z) = (worldPosition.x, worldPosition.y, worldPosition.z);
        float heightMapValue = _heightMap[x + z * 16]; // Assuming heightMap is a 1D array for a 16x16 chunk

        int surfaceHeight = (int)math.floor((heightMapValue + 1f) * 0.5f * maxY);

        if (worldPosition.y > surfaceHeight)
        {
            return _blockDatabase.GetNumericId("bunker:air_block");
        }
        else if (worldPosition.y == surfaceHeight)
        {
            return _blockDatabase.GetNumericId("bunker:grass_block");
        }
        else if (worldPosition.y > surfaceHeight - dirtDepth)
        {
            return _blockDatabase.GetNumericId("bunker:dirt_block");
        }
        else
        {
            return _blockDatabase.GetNumericId("bunker:stone_block");
        }
    }
}
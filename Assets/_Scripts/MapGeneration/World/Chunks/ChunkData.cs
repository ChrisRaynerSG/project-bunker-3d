using System;
using UnityEngine;

[Serializable]
public class ChunkData
{
    public const int CHUNK_SIZE = 16;
    public BlockData[][] Grid { get; private set; }

    public int ChunkX => OriginX / CHUNK_SIZE;
    public int ChunkZ => OriginZ / CHUNK_SIZE;


    public int OriginX { get; private set; }
    public int OriginZ { get; private set; }
    public int OriginY { get; private set; } // Y coordinate can be set later, default is 0
    public ChunkData(int originX, int originZ, int originY)
    {
        OriginX = originX;
        OriginZ = originZ;
        OriginY = originY;

        // Initialize the grid with CHUNK_SIZE x CHUNK_SIZE blocks
        Grid = new BlockData[CHUNK_SIZE][];
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            Grid[x] = new BlockData[CHUNK_SIZE];
            for (int z = 0; z < CHUNK_SIZE; z++)
            {
                Grid[x][z] = new BlockData
                {
                    X = originX + x,
                    Y = originY,
                    Z = originZ + z,
                    IsSolid = false // Default value, can be set later
                };
            }
        }
    }

}
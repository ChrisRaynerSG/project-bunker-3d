using System;

[Serializable]
public class VerticalSliceData
{

    public int Y { get; set; }
    //public bool IsVisible { get; set; }

    public ChunkData[][] Chunks { get; private set; }
    // public BlockData[][] Grid { get; private set; }

    // Constructor
    public VerticalSliceData(int y, int maxX, int maxZ)
    {

        Y = y;

        Chunks = new ChunkData[maxX / ChunkData.CHUNK_SIZE][];
        for (int x = 0; x < maxX / ChunkData.CHUNK_SIZE; x++)
        {
            Chunks[x] = new ChunkData[maxZ / ChunkData.CHUNK_SIZE];

            for (int z = 0; z < maxZ / ChunkData.CHUNK_SIZE; z++)
            {
                Chunks[x][z] = new ChunkData(x * ChunkData.CHUNK_SIZE, z * ChunkData.CHUNK_SIZE, y);
            }
        }


        //     Y = y;
        //     Grid = new BlockData[maxX][];
        //     for (int x = 0; x < maxX; x++)
        //     {

        //         Grid[x] = new BlockData[maxZ];

        //         for (int z = 0; z < maxZ; z++)
        //         {
        //             Grid[x][z] = new BlockData();
        //             Grid[x][z].X = x;
        //             Grid[x][z].Y = y;
        //             Grid[x][z].Z = z;
        //             Grid[x][z].IsSolid = false;
        //         }
        //     }
        // }
    }

}
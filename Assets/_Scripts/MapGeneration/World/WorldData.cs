using System;
using UnityEngine;
[Serializable]
public class WorldData
{
    private WorldData()
    {

    }

    // chunk by vertical slice so that we can go up and down and see underground areas with ease

    private VerticalSliceData[] ySlices;
    public VerticalSliceData[] YSlices
    {
        get { return ySlices; }
        set { ySlices = value; }
    }

    private BlockData[][,] grid;

    private static WorldData instance;
    public static WorldData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new WorldData();
            }
            return instance;
        }
    }

    public void Initialise(int maxX, int maxY, int maxZ, int minElevation)
    {
        int numberOfslices = maxY - minElevation;
        ySlices = new VerticalSliceData[numberOfslices];

        for (int i = 0; i < numberOfslices; i++)
        {
            ySlices[i] = new VerticalSliceData(i + minElevation, maxX, maxZ);
        }   
    }

    public void PrintWorldData()
    {
        Debug.Log("World Data:");
        for (int i = 0; i < ySlices.Length; i++)
        {
            Debug.Log($"Slice {i + 1} at Y={ySlices[i].Y}");
            for (int x = 0; x < ySlices[i].Chunks.Length; x++)
            {
                for (int z = 0; z < ySlices[i].Chunks[x].Length; z++)
                {
                    Debug.Log($"Chunk at ({x * ChunkData.CHUNK_SIZE}, {ySlices[i].Y}, {z * ChunkData.CHUNK_SIZE})");
                }
            }
        }
        
    }
}
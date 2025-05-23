using System;
using UnityEngine;
[Serializable]
public class WorldData
{
    private WorldData()
    {

    }

    // chunk by vertical slice so that we can go up and down and see underground areas with ease

    private VerticalSlice[] ySlices;
    public VerticalSlice[] YSlices
    {
        get { return ySlices; }
        set { ySlices = value; }
    }

    private WorldBlock[][,] grid;

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

    public void Initialise(int maxX, int maxY, int maxZ)
    {
        ySlices = new VerticalSlice[maxY];
        for (int y = 0; y < maxY; y++)
        {
            ySlices[y] = new VerticalSlice(y, maxX, maxZ);
        }
    }

    public void PrintWorldData()
    {
        for (int y = 0; y < ySlices.Length; y++)
        {
            Debug.Log("Vertical Slice: " + y);
            for (int x = 0; x < ySlices[y].Grid.Length; x++)
            {
                for (int z = 0; z < ySlices[y].Grid[x].Length; z++)
                {
                    Debug.Log("Block: " + ySlices[y].Grid[x][z].X + ", " + ySlices[y].Grid[x][z].Y + ", " + ySlices[y].Grid[x][z].Z);
                    Debug.Log("IsSolid: " + ySlices[y].Grid[x][z].IsSolid);
                }
            }
        }
    }
}
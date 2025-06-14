using System;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

/// <summary>
/// Stores all block and chunk data for the generated world, organized by vertical slices.
/// 
/// <see cref="WorldData"/> is a singleton that holds the world's structure, allowing for efficient access
/// and modification of blocks and chunks. The world is divided into vertical slices (Y levels), each containing
/// chunk data for that elevation. This structure supports features like underground exploration and dynamic
/// world updates.
/// </summary>
[Serializable]
public class WorldData
{
    /// <summary>
    /// Private constructor to enforce singleton pattern.
    /// </summary>
    private WorldData() { }

    /// <summary>
    /// Array of vertical slices, each representing a horizontal layer of the world.
    /// </summary>
    private VerticalSliceData[] ySlices;

    /// <summary>
    /// Gets or sets the array of vertical slices for the world.
    /// </summary>
    public VerticalSliceData[] YSlices
    {
        get { return ySlices; }
        set { ySlices = value; }
    }

    public List<TreeGameData> Trees = new();

    // (Unused) Grid for block data, can be removed or implemented as needed.
    private BlockData[][,] grid;

    /// <summary>
    /// Singleton instance of <see cref="WorldData"/>.
    /// </summary>
    private static WorldData instance;

    /// <summary>
    /// Gets the singleton instance of <see cref="WorldData"/>, creating it if necessary.
    /// </summary>
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

    /// <summary>
    /// Initializes the world data structure with the specified dimensions.
    /// </summary>
    /// <param name="maxX">Maximum X dimension of the world.</param>
    /// <param name="maxY">Maximum Y dimension of the world.</param>
    /// <param name="maxZ">Maximum Z dimension of the world.</param>
    /// <param name="minElevation">Minimum elevation (Y) of the world.</param>
    public void Initialise(int maxX, int maxY, int maxZ, int minElevation)
    {
        int numberOfslices = maxY - minElevation;
        Debug.Log($"Initialising WorldData with {numberOfslices} slices from {minElevation} to {maxY} (maxY={maxY})");
        ySlices = new VerticalSliceData[numberOfslices];

        Debug.Log($"Number of slices: {ySlices.Length}, MaxX: {maxX}, MaxZ: {maxZ}");

        for (int i = 0; i < numberOfslices; i++)
        {
            ySlices[i] = new VerticalSliceData(i + minElevation, maxX, maxZ);
        }
    }

    /// <summary>
    /// Prints a summary of the world data, including all slices and their chunks, to the debug log.
    /// </summary>
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

    public void AddTree(TreeGameData tree)
    {
        Trees.Add(tree);
    }
}
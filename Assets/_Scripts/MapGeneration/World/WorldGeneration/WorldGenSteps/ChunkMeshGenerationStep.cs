using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ChunkMeshGenerationStep is a world generation step that creates and instantiates chunk GameObjects,
/// generates their meshes, and attaches them to Y-slice parents in the scene hierarchy.
/// This step can be run as a coroutine to allow yielding after each Y slice for smoother loading.
/// </summary>
public class ChunkMeshGenerationStep : IWorldGenStep
{
    /// <summary>
    /// Synchronous mesh generation for all Y slices and chunks.
    /// </summary>
    /// <param name="data">The world data structure to read from.</param>
    /// <param name="context">The context containing world generation parameters and dependencies.</param>
    public void Apply(WorldData data, WorldGenContext context)
    {
        int minElevation = context.minElevation;
        int maxY = context.maxY;

        for (int y = minElevation; y < maxY; y++)
        {
            GenerateYSlice(data, context, y);
        }
    }

    /// <summary>
    /// Coroutine-based mesh generation for all Y slices and chunks.
    /// Yields after each Y slice is generated.
    /// </summary>
    /// <param name="data">The world data structure to read from.</param>
    /// <param name="context">The context containing world generation parameters and dependencies.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public IEnumerator ApplyCoroutine(WorldData data, WorldGenContext context)
    {
        int minElevation = context.minElevation;
        int maxY = context.maxY;

        for (int y = minElevation; y < maxY; y++)
        {
            GenerateYSlice(data, context, y);
            yield return null; // Yield after each Y slice for smoother loading
        }
    }

    /// <summary>
    /// Generates a single Y slice, instantiating its GameObject and all chunk GameObjects within it.
    /// </summary>
    private void GenerateYSlice(WorldData data, WorldGenContext context, int y)
    {
        GameObject ySliceObject = Object.Instantiate(context.ySlicePrefab, context.world.transform);
        ySliceObject.name = $"YSlice_{y}";
        ySliceObject.transform.position = new Vector3(0, y, 0);
        context.ySlices.Add(ySliceObject);

        int chunkXCount = context.maxX / ChunkData.CHUNK_SIZE;
        int chunkZCount = context.maxZ / ChunkData.CHUNK_SIZE;

        for (int chunkX = 0; chunkX < chunkXCount; chunkX++)
        {
            for (int chunkZ = 0; chunkZ < chunkZCount; chunkZ++)
            {
                GenerateChunk(data, context, ySliceObject.transform, y, chunkX, chunkZ);
            }
        }
    }

    /// <summary>
    /// Generates a single chunk GameObject, builds its mesh, and attaches it to the given parent.
    /// </summary>
    private void GenerateChunk(
    WorldData data,
    WorldGenContext context,
    Transform parent,
    int y,
    int chunkX,
    int chunkZ)
    {
        GameObject chunkObject = Object.Instantiate(context.chunkPrefab, parent);
        chunkObject.name = $"Chunk_{chunkX}_{chunkZ}_{y}";
        chunkObject.transform.position = new Vector3(chunkX * ChunkData.CHUNK_SIZE, 0, chunkZ * ChunkData.CHUNK_SIZE);

        // Optionally, register the chunkObject in your ySlices structure here if needed

        // Use ChunkUtils to build the mesh for this chunk
        // Use the world coordinates of the first block in the chunk
        int worldX = chunkX * ChunkData.CHUNK_SIZE;
        int worldZ = chunkZ * ChunkData.CHUNK_SIZE;
        ChunkUtils.RebuildChunkMesh(worldX, y, worldZ);
        
        chunkObject.layer = 0;
    }
}
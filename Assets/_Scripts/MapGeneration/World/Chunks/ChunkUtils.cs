using UnityEngine;

/// <summary>
/// Utility methods for chunk lookup and mesh rebuilding in the voxel world.
/// </summary>
public static class ChunkUtils
{
    /// <summary>
    /// Gets the chunk at the specified world position.
    /// </summary>
    /// <param name="x">World X coordinate.</param>
    /// <param name="y">World Y coordinate.</param>
    /// <param name="z">World Z coordinate.</param>
    /// <returns>The <see cref="ChunkData"/> at the given position, or null if out of bounds.</returns>
    public static ChunkData GetChunkAtPosition(int x, int y, int z)
    {
        int chunkX = x / ChunkData.CHUNK_SIZE;
        int chunkZ = z / ChunkData.CHUNK_SIZE;

        if (chunkX < 0 || chunkZ < 0 || chunkX >= World.Instance.ChunkXCount || chunkZ >= World.Instance.ChunkZCount)
            return null;

        int yIndex = y - World.Instance.minElevation;
        if (yIndex < 0 || yIndex >= WorldData.Instance.YSlices.Length)
            return null;

        return WorldData.Instance.YSlices[yIndex].Chunks[chunkX][chunkZ];
    }

    /// <summary>
    /// Gets the chunk at the specified world position.
    /// </summary>
    /// <param name="position">World position as a Vector3Int.</param>
    /// <returns>The <see cref="ChunkData"/> at the given position, or null if out of bounds.</returns>
    public static ChunkData GetChunkAtPosition(Vector3Int position)
    {
        return GetChunkAtPosition(position.x, position.y, position.z);
    }

    /// <summary>
    /// Builds the mesh for a single chunk GameObject.
    /// </summary>
    /// <param name="chunkX">Chunk X index.</param>
    /// <param name="chunkZ">Chunk Z index.</param>
    /// <param name="y">World Y coordinate.</param>
    /// <param name="yIndex">Y slice index.</param>
    /// <param name="originY">Chunk's origin Y coordinate.</param>
    /// <param name="chunkObject">The chunk GameObject to update.</param>
    /// <param name="forceTop">If true, always creates the top face.</param>
    private static void BuildChunkMesh(
        int chunkX, int chunkZ, int y, int yIndex, int originY, GameObject chunkObject, bool forceTop = false)
    {
        MeshFilter chunkFilter = chunkObject.GetComponent<MeshFilter>();
        MeshData meshData = new MeshData();

        for (int x = 0; x < ChunkData.CHUNK_SIZE; x++)
        {
            for (int z = 0; z < ChunkData.CHUNK_SIZE; z++)
            {
                int worldX = chunkX * ChunkData.CHUNK_SIZE + x;
                int worldZ = chunkZ * ChunkData.CHUNK_SIZE + z;

                if (worldX < World.Instance.maxX && worldZ < World.Instance.maxZ && BlockUtils.IsSolid(worldX, originY, worldZ))
                {
                    BlockData blockData = WorldData.Instance.YSlices[yIndex].Chunks[chunkX][chunkZ].Grid[x][z];
                    Vector3 targetPosition = new Vector3(x, originY, z);
                    MeshUtilities.CreateFaces(y, meshData, worldX, worldZ, targetPosition, blockData, forceTop);
                }
            }
        }
        MeshUtilities.LoadMeshData(meshData, chunkFilter);
        var collider = chunkObject.GetComponent<MeshCollider>();
        if (collider != null)
            collider.sharedMesh = chunkFilter.mesh;
    }

    /// <summary>
    /// Rebuilds the mesh for all chunks at the specified Y level.
    /// </summary>
    /// <param name="y">The world Y coordinate (layer) to rebuild.</param>
    /// <param name="forceTop">If true, always creates the top face for blocks.</param>
    public static void RebuildMeshAtLevel(int y, bool forceTop = false)
    {
        GameObject ySliceObject = World.Instance.YSlices[y - World.Instance.minElevation];
        for (int chunkX = 0; chunkX < World.Instance.ChunkXCount; chunkX++)
        {
            for (int chunkZ = 0; chunkZ < World.Instance.ChunkZCount; chunkZ++)
            {
                GameObject chunkObject = ySliceObject.transform.GetChild(chunkX * World.Instance.ChunkZCount + chunkZ).gameObject;
                BuildChunkMesh(chunkX, chunkZ, y, y - World.Instance.minElevation, y, chunkObject, forceTop);
            }
        }
    }

    /// <summary>
    /// Rebuilds the mesh for a single chunk at the specified world position.
    /// </summary>
    /// <param name="x">World X coordinate.</param>
    /// <param name="y">World Y coordinate.</param>
    /// <param name="z">World Z coordinate.</param>
    public static void RebuildChunkMesh(int x, int y, int z, bool forceTop = false)
    {
        ChunkData chunkData = GetChunkAtPosition(x, y, z);
        if (chunkData == null) return;

        int chunkX = chunkData.ChunkX;
        int chunkZ = chunkData.ChunkZ;
        int yIndex = chunkData.OriginY - World.Instance.minElevation;

        if (yIndex < 0 || yIndex >= WorldData.Instance.YSlices.Length) return;

        GameObject chunkObject = World.Instance.YSlices[yIndex].transform.Find($"Chunk_{chunkX}_{chunkZ}_{chunkData.OriginY}").gameObject;
        if (chunkObject == null) return;

        BuildChunkMesh(chunkX, chunkZ, chunkData.OriginY, yIndex, chunkData.OriginY, chunkObject, forceTop);
    }
}
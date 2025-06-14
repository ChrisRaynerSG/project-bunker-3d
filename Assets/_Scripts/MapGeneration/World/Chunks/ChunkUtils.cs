using UnityEngine;

public static class ChunkUtils
{
    /// <summary>
    /// Checks if the given position is within the bounds of the chunk.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is within bounds, false otherwise.</returns>

    public static ChunkData GetChunkAtPosition(int x, int y, int z)
    {
        int chunkX = x / ChunkData.CHUNK_SIZE;
        int chunkZ = z / ChunkData.CHUNK_SIZE;
        int chunkLocalX = x % ChunkData.CHUNK_SIZE;
        int chunkLocalZ = z % ChunkData.CHUNK_SIZE;

        if (chunkX < 0 || chunkZ < 0 || chunkX >= World.Instance.ChunkXCount || chunkZ >= World.Instance.ChunkZCount)
            return null;

        int yIndex = y - World.Instance.minElevation;
        if (yIndex < 0 || yIndex >= WorldData.Instance.YSlices.Length)
            return null;

        return WorldData.Instance.YSlices[yIndex].Chunks[chunkX][chunkZ];
    }

    public static ChunkData GetChunkAtPosition(Vector3Int position)
    {
        return GetChunkAtPosition(position.x, position.y, position.z);
    }

    private static void BuildChunkMesh(
    int chunkX, int chunkZ, int y, int yIndex, int originY, GameObject chunkObject)
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
                    if (World.Instance.currentElevation == originY)
                    {
                        MeshUtilities.CreateFaceUp(meshData, targetPosition, blockData.definition);
                    }
                    else
                    {
                        if (!BlockUtils.IsSolid(worldX, originY + 1, worldZ))
                            MeshUtilities.CreateFaceUp(meshData, targetPosition, blockData.definition);
                    }
                    if (!BlockUtils.IsSolid(worldX, originY - 1, worldZ)) MeshUtilities.CreateFaceDown(meshData, targetPosition, blockData.definition);
                    if (!BlockUtils.IsSolid(worldX, originY, worldZ + 1)) MeshUtilities.CreateFaceNorth(meshData, targetPosition, blockData.definition);
                    if (!BlockUtils.IsSolid(worldX + 1, originY, worldZ)) MeshUtilities.CreateFaceEast(meshData, targetPosition, blockData.definition);
                    if (!BlockUtils.IsSolid(worldX, originY, worldZ - 1)) MeshUtilities.CreateFaceSouth(meshData, targetPosition, blockData.definition);
                    if (!BlockUtils.IsSolid(worldX - 1, originY, worldZ)) MeshUtilities.CreateFaceWest(meshData, targetPosition, blockData.definition);
                }
            }
        }
        MeshUtilities.LoadMeshData(meshData, chunkFilter);
        chunkObject.GetComponent<MeshCollider>().sharedMesh = chunkFilter.mesh;
    }

    public static void RebuildMeshAtLevel(int y)
    {
        GameObject ySliceObject = World.Instance.YSlices[y - World.Instance.minElevation];
        for (int chunkX = 0; chunkX < World.Instance.ChunkXCount; chunkX++)
        {
            for (int chunkZ = 0; chunkZ < World.Instance.ChunkZCount; chunkZ++)
            {
                GameObject chunkObject = ySliceObject.transform.GetChild(chunkX * World.Instance.ChunkZCount + chunkZ).gameObject;
                BuildChunkMesh(chunkX, chunkZ, y, y - World.Instance.minElevation, y, chunkObject);
            }
        }
    }

    public static void RebuildChunkMesh(int x, int y, int z)
    {
        ChunkData chunkData = ChunkUtils.GetChunkAtPosition(x, y, z);
        if (chunkData == null) return;

        int chunkX = chunkData.ChunkX;
        int chunkZ = chunkData.ChunkZ;
        int yIndex = chunkData.OriginY - World.Instance.minElevation;

        if (yIndex < 0 || yIndex >= WorldData.Instance.YSlices.Length) return;

        GameObject chunkObject = World.Instance.YSlices[yIndex].transform.Find($"Chunk_{chunkX}_{chunkZ}_{chunkData.OriginY}").gameObject;
        if (chunkObject == null) return;

        BuildChunkMesh(chunkX, chunkZ, chunkData.OriginY, yIndex, chunkData.OriginY, chunkObject);
    }
    


}
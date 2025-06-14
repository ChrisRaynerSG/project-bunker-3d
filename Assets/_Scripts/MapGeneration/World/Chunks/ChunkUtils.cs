using UnityEngine;

public static class ChunkUtils
{
    /// <summary>
    /// Checks if the given position is within the bounds of the chunk.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is within bounds, false otherwise.</returns>
    /// 

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

}
public static class BlockUtils
{
    public static bool IsSolid(int x, int y, int z)
    {
        if (x < 0 || x >= World.Instance.maxX || z < 0 || z >= World.Instance.maxZ || y < World.Instance.minElevation || y >= World.Instance.maxY)
            return false;

        int yIndex = y - World.Instance.minElevation;
        int chunkX = x / ChunkData.CHUNK_SIZE;
        int chunkZ = z / ChunkData.CHUNK_SIZE;
        int chunkLocalX = x % ChunkData.CHUNK_SIZE;
        int chunkLocalZ = z % ChunkData.CHUNK_SIZE;

        return WorldData.Instance.YSlices[yIndex].Chunks[chunkX][chunkZ].Grid[chunkLocalX][chunkLocalZ].definition.isSolid;
    }
}
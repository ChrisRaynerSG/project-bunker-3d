using UnityEngine.UIElements;
using UnityEngine;
public class BlockAccessor
{
    private World world;

    public BlockAccessor(World world)
    {
        this.world = world;
    }

    // public void SetBlock(Vector3Int position, BlockData.BlockType blockType)
    // {
    //     ChunkData chunk = world.GetChunkAtWorldPosition(position.x, position.z);
    //     if (chunk != null)
    //     {
    //         int localX = position.x % ChunkData.CHUNK_SIZE;
    //         int localZ = position.z % ChunkData.CHUNK_SIZE;

    //         if (localX < 0) localX += ChunkData.CHUNK_SIZE;
    //         if (localZ < 0) localZ += ChunkData.CHUNK_SIZE;

    //         BlockData block = chunk.Grid[localX][localZ];
    //         block.BlockTypeName = blockType.ToString();
    //         block.IsSolid = blockType != BlockType.Air; // Example logic for solid blocks
    //     }
    // }

    public void SetBlock(Vector3Int position, BlockData.BlockType blockType)
    {
        BlockData block = GetBlock(position);
        if (block != null)
        {
            block.BlockTypeName = blockType.ToString();
            block.IsSolid = blockType != BlockData.BlockType.Air;
            
            world.RebuildMeshAtLevel(position.y);
            world.RebuildMeshAtLevel(position.y + 1); // Rebuild the mesh at the block's Y level
            world.RebuildMeshAtLevel(position.y - 1); // Rebuild the mesh at the block's Y level
        }

    
    }

    public BlockData GetBlock(Vector3Int position)
    {
        ChunkData chunk = world.GetChunkAtPosition(position.x, position.y, position.z);
        if (chunk != null)
        {
            int localX = position.x % ChunkData.CHUNK_SIZE;
            int localZ = position.z % ChunkData.CHUNK_SIZE;

            if (localX < 0) localX += ChunkData.CHUNK_SIZE;
            if (localZ < 0) localZ += ChunkData.CHUNK_SIZE;

            // Debug.Log($"Getting block at local coordinates: ({localX}, {localZ}) in chunk at ({chunk.ChunkX}, {chunk.ChunkZ})");

            return chunk.Grid[localX][localZ];
        }
        return null;
    }
}
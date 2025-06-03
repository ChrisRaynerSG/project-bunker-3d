using UnityEngine.UIElements;
using UnityEngine;
public class BlockAccessor
{
    private World world;

    public BlockAccessor(World world)
    {
        this.world = world;
    }

    public void SetBlock(Vector3Int position, BlockType blockType)
    {
        BlockData block = GetBlock(position);
        if (block != null)
        {
            block.type = blockType;
            block.IsSolid = blockType != BlockType.Air;

            // world.RebuildMeshAtLevel(position.y);
            // world.RebuildMeshAtLevel(position.y + 1); // Rebuild the mesh at the block's Y level
            // world.RebuildMeshAtLevel(position.y - 1); // Rebuild the mesh at the block's Y level

            // attempt to reubild only the chunk that contains the block - might not work with adjacent chunks...
            world.RebuildChunkMesh(position.x, position.y, position.z);
            world.RebuildChunkMesh(position.x, position.y + 1, position.z);
            world.RebuildChunkMesh(position.x, position.y - 1, position.z);

            // need to rebuild chunks next to the current chunk if on border
            int chunkX = position.x / ChunkData.CHUNK_SIZE;
            int chunkZ = position.z / ChunkData.CHUNK_SIZE;
            int localX = position.x % ChunkData.CHUNK_SIZE;
            int localZ = position.z % ChunkData.CHUNK_SIZE;

            if (localX == 0)
            {
                world.RebuildChunkMesh(position.x -1, position.y, position.z);
            }
            else if (localX == ChunkData.CHUNK_SIZE - 1)
            {
                world.RebuildChunkMesh(position.x+1, position.y, position.z);
            }
            if (localZ == 0)
            {
                world.RebuildChunkMesh(position.x, position.y, position.z - 1);
            }
            else if (localZ == ChunkData.CHUNK_SIZE - 1)
            {
                world.RebuildChunkMesh(position.x, position.y, position.z + 1);
            }
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
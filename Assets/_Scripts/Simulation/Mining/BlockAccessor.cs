using UnityEngine;

/// <summary>
/// Provides methods for accessing and modifying block data within a specific <see cref="World"/> instance.
/// 
/// BlockAccessor allows setting and retrieving blocks at given world positions, with or without triggering mesh updates.
/// It also provides utility methods for retrieving block definitions from the block database.
/// </summary>
public class BlockAccessor
{
    private World world;
    private BlockDatabase blockDatabase;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockAccessor"/> class for the specified world.
    /// </summary>
    /// <param name="world">The world instance to operate on.</param>
    public BlockAccessor(World world)
    {
        this.world = world;
        this.blockDatabase = BlockDatabase.Instance;
    }

    /// <summary>
    /// Sets the block at the given position without updating the mesh.
    /// </summary>
    /// <param name="position">The world position of the block.</param>
    /// <param name="blockType">The block definition to set.</param>
    public void SetBlockNoMeshUpdate(Vector3Int position, BlockDefinition blockType)
    {
        BlockData block = GetBlockDataFromPosition(position);
        if (block != null)
        {
            block.definition = blockType;
            block.IsSolid = blockType.isSolid;
        }
    }

    /// <summary>
    /// Sets the block at the given position and updates the mesh for the affected chunks.
    /// </summary>
    /// <param name="position">The world position of the block.</param>
    /// <param name="blockType">The block definition to set.</param>
    public void SetBlock(Vector3Int position, BlockDefinition blockType)
    {
        BlockData block = GetBlockDataFromPosition(position);
        if (block != null)
        {
            block.definition = blockType;
            block.IsSolid = blockType.isSolid;

            bool forceTop = position.y == world.currentElevation;

            ChunkUtils.RebuildChunkMesh(position.x, position.y, position.z, forceTop);
            ChunkUtils.RebuildChunkMesh(position.x, position.y + 1, position.z, forceTop);
            ChunkUtils.RebuildChunkMesh(position.x, position.y - 1, position.z, forceTop);

            int chunkX = position.x / ChunkData.CHUNK_SIZE;
            int chunkZ = position.z / ChunkData.CHUNK_SIZE;
            int localX = position.x % ChunkData.CHUNK_SIZE;
            int localZ = position.z % ChunkData.CHUNK_SIZE;

            if (localX == 0)
                ChunkUtils.RebuildChunkMesh(position.x - 1, position.y, position.z, forceTop);
            else if (localX == ChunkData.CHUNK_SIZE - 1)
                ChunkUtils.RebuildChunkMesh(position.x + 1, position.y, position.z, forceTop);

            if (localZ == 0)
                ChunkUtils.RebuildChunkMesh(position.x, position.y, position.z - 1, forceTop);
            else if (localZ == ChunkData.CHUNK_SIZE - 1)
                ChunkUtils.RebuildChunkMesh(position.x, position.y, position.z + 1, forceTop);
        }
    }

    /// <summary>
    /// Retrieves the <see cref="BlockData"/> at the specified world coordinates.
    /// </summary>
    /// <param name="x">The world X coordinate.</param>
    /// <param name="y">The world Y coordinate.</param>
    /// <param name="z">The world Z coordinate.</param>
    /// <returns>The block data at the specified position, or null if not found.</returns>
    public BlockData GetBlockDataFromPosition(int x, int y, int z)
    {
        return GetBlockDataFromPosition(new Vector3Int(x, y, z));
    }

    /// <summary>
    /// Retrieves the <see cref="BlockData"/> at the specified world position.
    /// </summary>
    /// <param name="position">The world position of the block.</param>
    /// <returns>The block data at the specified position, or null if not found.</returns>
    public BlockData GetBlockDataFromPosition(Vector3Int position)
    {
        ChunkData chunk = ChunkUtils.GetChunkAtPosition(position);
        if (chunk != null)
        {
            int localX = position.x % ChunkData.CHUNK_SIZE;
            int localZ = position.z % ChunkData.CHUNK_SIZE;

            if (localX < 0) localX += ChunkData.CHUNK_SIZE;
            if (localZ < 0) localZ += ChunkData.CHUNK_SIZE;

            return chunk.Grid[localX][localZ];
        }
        return null;
    }

    /// <summary>
    /// Retrieves a <see cref="BlockDefinition"/> by its unique identifier.
    /// </summary>
    /// <param name="blockId">The unique identifier of the block.</param>
    /// <returns>The block definition if found; otherwise, null.</returns>
    public BlockDefinition GetBlockDef(string blockId)
    {
        BlockDefinition block = blockDatabase.GetBlockDefinition(blockId);
        if (block == null)
        {
            Debug.LogError($"Block with ID {blockId} not found in the database.");
        }
        return block;
    }
}
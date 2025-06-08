using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct ChunkBlockGenerationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ChunkTag>();
        state.RequireForUpdate<ChunkPosition>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (chunkPosition, entity) in SystemAPI.Query<RefRO<ChunkPosition>>()
        .WithNone<ChunkBlocksInitialisedTag>()
        .WithEntityAccess())
        {
            var buffer = SystemAPI.GetBuffer<Block>(entity);
            var bufferSize = WorldConstants.CHUNK_SIZE_X * WorldConstants.CHUNK_SIZE_Y * WorldConstants.CHUNK_SIZE_Z;

            if (buffer.Length == bufferSize)
            {
                continue;
            }

            buffer.Clear(); // Clear the buffer before filling it
            buffer.ResizeUninitialized(bufferSize);

            int index = 0;
            for (int x = 0; x < WorldConstants.CHUNK_SIZE_X; x++)
            {
                for (int y = 0; y < WorldConstants.CHUNK_SIZE_Y; y++)
                {
                    for (int z = 0; z < WorldConstants.CHUNK_SIZE_Z; z++)
                    {
                        // setup all blocks in the chunk to default values;
                        buffer[index++] = new Block
                        {
                            Id = 0, // need to assign the id of the block based on world generation rules
                            Temperature = 21.0f,
                            Radiation = 0.0f
                        };
                    }
                }
            }
            state.EntityManager.AddComponent<ChunkBlocksInitialisedTag>(entity); // Mark the chunk as initialised so we don't do this again
        } 
    }
}
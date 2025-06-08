using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Analytics;

[BurstCompile]
public partial struct ChunkBlockGenerationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ChunkTag>();
        state.RequireForUpdate<ChunkPosition>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (chunkPosition, entity) in SystemAPI.Query<RefRO<ChunkPosition>>().WithEntityAccess())
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
                        buffer[index++] = new Block
                        {
                            Id = 0, // Placeholder for block ID, will be set later
                            Temperature = 20.0f, // Example temperature, replace with actual logic
                            Radiation = 0.0f // Example radiation, replace with actual logic
                        };
                    }
                }
            }
        } 
    }
}
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(NoiseGenerationSystem))]

public partial struct ChunkGenerationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldTag>();
        state.RequireForUpdate<Seed>();
        state.RequireForUpdate<NoiseGeneratedTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // This system is responsible for generating chunks based on the world noise and seed
        // It will create chunks we will burst compile blocks later
        // Check if the world has already generated chunks
        if (SystemAPI.HasSingleton<ChunksGeneratedTag>())
        {
            return; // Skip chunk generation if chunks are already generated
        }

        var worldEntity = SystemAPI.GetSingletonEntity<WorldTag>();
        if (!SystemAPI.HasComponent<NoiseGeneratedTag>(worldEntity))
        {
            UnityEngine.Debug.LogWarning("Noise has not been generated yet. Skipping chunk generation.");
            return; // Skip chunk generation if noise has not been generated
        }

        int worldSize = WorldConstants.WORLD_SIZE;
        int chunkSize = WorldConstants.CHUNK_SIZE_X; // Assuming square chunks for simplicity
        int numChunks = worldSize / chunkSize;
        int numChunksY = 64 / chunkSize;
        if (worldSize % chunkSize != 0)
        {
            UnityEngine.Debug.LogError("World size is not divisible by chunk size. Cannot generate chunks.");
            return; // Ensure world size is divisible by chunk size
        }

        var archetype = state.EntityManager.CreateArchetype(
            typeof(ChunkTag),
            typeof(ChunkPosition),
            typeof(LocalToWorld)
            
        );

        for (int x = 0; x < numChunks; x++)
        {
            for (int y = 0; y < numChunksY; y++)
            {
                for (int z = 0; z < numChunks; z++)
                {
                    // Calculate the position of the chunk
                    var chunkPosition = new int3(x * chunkSize, y * chunkSize, z * chunkSize);

                    // Create a new chunk entity
                    var chunkEntity = state.EntityManager.CreateEntity(archetype);

                    state.EntityManager.AddBuffer<Block>(chunkEntity);

                    // Set the chu
                    state.EntityManager.SetComponentData(chunkEntity, new ChunkPosition { Value = chunkPosition });
                    // Optionally, you can set the LocalToWorld component here if needed
                    state.EntityManager.SetComponentData(chunkEntity, new LocalToWorld
                    {
                        Value = float4x4.TRS(
                            new float3(chunkPosition.x, chunkPosition.y, chunkPosition.z),
                            quaternion.identity,
                            new float3(1f, 1f, 1f)
                        )
                    });
                    // Optionally, you can initialize other components or buffers here
                }
            }
        }
            // After generating chunks, we mark the world as having generated chunks
            state.EntityManager.AddComponent<ChunksGeneratedTag>(SystemAPI.GetSingletonEntity<WorldTag>());
    }
}
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Burst;
using Unity.Entities.UniversalDelegates;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(NoiseGenerationSystem))]
[BurstCompile]
public partial struct ChunkGenerationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldTag>();
        state.RequireForUpdate<Seed>();
        state.RequireForUpdate<NoiseGeneratedTag>();
    }

    [BurstCompile]
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

        int worldSize = SystemAPI.GetSingleton<WorldSize>().Value;
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
            typeof(LocalToWorld),
            typeof(DirtyChunkTag),
            typeof(RenderMeshArray),
            typeof(MaterialMeshInfo),
            typeof(WorldRenderBounds)
        );

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        for (int x = 0; x < numChunks; x++)
        {
            for (int y = -numChunksY; y < numChunksY; y++)
            {
                for (int z = 0; z < numChunks; z++)
                {
                    // Calculate the position of the chunk
                    var chunkPosition = new int3(x * chunkSize, y * chunkSize, z * chunkSize);

                    // Create a new entity for the chunk
                    // We use the EntityCommandBuffer to create entities in a safe way
                    var chunkEntity = ecb.CreateEntity(archetype);

                    ecb.AddBuffer<Block>(chunkEntity);

                    // Set the chu
                    ecb.SetComponent(chunkEntity, new ChunkPosition { Value = chunkPosition });
                    // Optionally, you can set the LocalToWorld component here if needed
                    ecb.SetComponent(chunkEntity, new LocalToWorld
                    {
                        Value = float4x4.TRS(
                            new float3(chunkPosition.x, chunkPosition.y, chunkPosition.z),
                            quaternion.identity,
                            new float3(1f, 1f, 1f)
                        )
                    });
                    // ecb.AddComponent<DirtyChunkTag>(chunkEntity); // Add DirtyChunkTag to the chunk entity
                    ecb.SetComponentEnabled<DirtyChunkTag>(chunkEntity, false); // Adding DirtyChunkTag but disabling it initially
                    ecb.SetName(chunkEntity, $"Chunk_{x}_{y}_{z}");
                    
                    // Optionally, you can initialize other components or buffers here
                }
            }
        }
        // After generating chunks, we mark the world as having generated chunks

        ecb.Playback(state.EntityManager);
        ecb.Dispose(); // Dispose of the command buffer to free resources
        state.EntityManager.AddComponent<ChunksGeneratedTag>(SystemAPI.GetSingletonEntity<WorldTag>());
        UnityEngine.Debug.Log($"Generated {numChunks * numChunksY * numChunks} chunks in the world.");

        // disable this system after generating chunks
        state.Enabled = false; // Disable this system after chunk generation
        UnityEngine.Debug.Log("ChunkGenerationSystem has completed chunk generation and is now disabled.");
    }
}
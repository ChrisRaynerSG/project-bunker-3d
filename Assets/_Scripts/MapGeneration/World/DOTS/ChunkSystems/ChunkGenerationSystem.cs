using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(NoiseGenerationSystem))]
[BurstCompile]
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
        // foreach(ChunksGeneratedTag tagRef in SystemAPI.Query<RefRO<ChunksGeneratedTag>>())
        // {
        //     if (tagRef.)
        //     {
        //         UnityEngine.Debug.Log("Chunks have already been generated. Skipping ChunkGenerationSystem.");
        //         state.Enabled = false; // Disable this system if chunks are already generated
        //         return;
        //     }
        // }

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
            typeof(LocalToWorld),
            typeof(DirtyChunkTag),
            typeof(ChunkBlocksInitialisedTag)

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
                    ecb.SetName(chunkEntity, $"Chunk_{x}_{y}_{z}");
                    ecb.SetComponentEnabled<ChunkBlocksInitialisedTag>(chunkEntity, false); // add a tag to indicate that the chunk blocks are not initialized yet
                    ecb.SetComponentEnabled<DirtyChunkTag>(chunkEntity, true); // add a tag to indicate that the chunk is dirty and needs to be processed
                    // Optionally, you can initialize other components or buffers here
                }
            }
        }
        // After generating chunks, we mark the world as having generated chunks

        ecb.Playback(state.EntityManager);
        ecb.Dispose(); // Dispose of the command buffer to free resources
                       // state.EntityManager.AddComponent<ChunksGeneratedTag>(SystemAPI.GetSingletonEntity<WorldTag>());
                       // need to set the ChunksGeneratedTag to true to indicate that chunks have been generated
        foreach (var (tagRef, entity) in SystemAPI.Query<RefRO<ChunksGeneratedTag>>().WithEntityAccess())
        {
            // should only be one world entity so lets set chunks generated to true
            state.EntityManager.SetComponentEnabled<ChunksGeneratedTag>(entity, true);
        }
        UnityEngine.Debug.Log($"Generated {numChunks * numChunksY * numChunks} chunks in the world.");

        // disable this system after generating chunks
        state.Enabled = false; // Disable this system after chunk generation
        UnityEngine.Debug.Log("ChunkGenerationSystem has completed chunk generation and is now disabled.");
    }
}
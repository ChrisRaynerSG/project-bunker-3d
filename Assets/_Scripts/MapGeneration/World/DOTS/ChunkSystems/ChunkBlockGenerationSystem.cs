using Unity.Entities;
using Unity.Burst;
using System.Diagnostics;
using System;
using JetBrains.Annotations;
using Unity.Entities.UniversalDelegates;
using Unity.Collections;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ChunkGenerationSystem))]
[UpdateAfter(typeof(BlockLoadingSystem))]
public partial struct ChunkBlockGenerationSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ChunkTag>();
        state.RequireForUpdate<ChunkPosition>();
        state.RequireForUpdate<ChunksGeneratedTag>();

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        bool allInitialised = true;
        BlockGenerationJob job = new BlockGenerationJob
        {
            ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            heightNoiseBuffer = SystemAPI.GetSingletonBuffer<HeightNoise>().AsNativeArray(),
            worldSize = SystemAPI.GetSingleton<WorldSize>().Value
        };

        job.ScheduleParallel();

        foreach (var _ in SystemAPI.Query<RefRO<ChunkPosition>>()
            .WithNone<ChunkBlocksInitialisedTag>())
        {
            allInitialised = false;
            break;
            // This is where we would normally handle the case where chunks are not initialized
            // but since we are using a job, we don't need to do anything here.
        }

        if (allInitialised)
        {
            state.Enabled = false; // Disable this system if all chunks are initialized
            UnityEngine.Debug.Log("All chunks have been initialized with blocks.");
            return;
        }
        
        /*
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
        }
    }
    */
    }
}

[BurstCompile]
[WithNone(typeof(ChunkBlocksInitialisedTag))]
public partial struct BlockGenerationJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    [ReadOnly] public NativeArray<HeightNoise> heightNoiseBuffer;

    public int worldSize;

    public void Execute([EntityIndexInChunk] int entityIndex, Entity entity, in ChunkPosition chunkPosition, ref DynamicBuffer<Block> blocks)
    {


        var bufferSize = WorldConstants.CHUNK_SIZE_X * WorldConstants.CHUNK_SIZE_Y * WorldConstants.CHUNK_SIZE_Z;

        if (blocks.Length == bufferSize)
        {
            UnityEngine.Debug.LogWarning($"Chunk at {chunkPosition.Value} already has blocks initialized.");
            return;
        }

        blocks.Clear(); // Clear the buffer before filling it
        blocks.ResizeUninitialized(bufferSize);

        int index = 0;
        for (int x = 0; x < WorldConstants.CHUNK_SIZE_X; x++)
        {
            for (int z = 0; z < WorldConstants.CHUNK_SIZE_Z; z++)
            {
                int worldX = chunkPosition.Value.x + x;
                int worldZ = chunkPosition.Value.z + z;
                int surfaceY = heightNoiseBuffer[worldX + worldZ * worldSize].Value; // Get the surface height for this x,z coordinate

                for (int y = 0; y < WorldConstants.CHUNK_SIZE_Y; y++)
                {
                    int worldY = chunkPosition.Value.y + y;
                    ushort blockId;

                    if (worldY < surfaceY)
                    {
                        // Below the surface, we have dirt and stone
                        if (worldY < surfaceY - WorldConstants.DIRT_LEVEL)
                        {
                            blockId = 1; // Stone block
                        }
                        else
                        {
                            blockId = 2; // Dirt block
                        }
                    }
                    else if (worldY == surfaceY)
                    {
                        blockId = 3; // Grass block at the surface
                    }
                    else
                    {
                        blockId = 0; // Air block above the surface
                    }

                    // definitely need a better way to handle block ids, need to lookup the block id in a dictionary or something
                    // setup all blocks in the chunk to default values;
                    blocks[index++] = new Block
                    {
                        Id = blockId, // need to assign the id of the block based on world generation rules
                        Temperature = 21.0f,
                        Radiation = 0.0f
                    };
                }
            }
        }
        ecb.AddComponent<ChunkBlocksInitialisedTag>(entityIndex, entity);
    }
}

// noise is currently a 1d array of height values for the world using x,z coordinates
// for each x,z coordinate we have a height value that indicates the y coordinate of the surface terrain
// anything above this height is considered air, at this height we have grass, and below we have dirt for x blocks,
// followed by stone for the rest of the journey down to the bottom of the world
// we will need to generate the blocks in the chunks based on this noise data


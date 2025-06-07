using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;

namespace Bunker.World
{
    public struct ChunkTag : IComponentData { } // Tag component to identify chunk entities
    public struct DirtyChunkTag : IComponentData { } // Tag component to identify dirty chunk entities, which need to be updated

    public struct ChunkPosition : IComponentData
    {
        public int3 Value; // Position of the chunk in the world
    }
    public struct Block : IBufferElementData // Buffer element data to store blocks in a chunk
    {
        public ushort BlockId;
    }

    public class ChunkAuthoring : MonoBehaviour
    {
        public int3 chunkPosition;

        private class ChunkBaker : Baker<ChunkAuthoring>
        {
            public override void Bake(ChunkAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None); //None because this wont move, doesn't need to be dynamic
                AddComponent(entity, new ChunkTag());
                AddComponent(entity, new ChunkPosition { Value = authoring.chunkPosition });
                AddBuffer<Block>(entity);

            }
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct ChunkGenerationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChunkTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (chunkPosition, entity) in SystemAPI.Query<RefRO<ChunkPosition>>().WithEntityAccess())
            {
                DynamicBuffer<Block> blocks = SystemAPI.GetBuffer<Block>(entity);

                blocks.Clear(); // clear blocks before starting generation

                for (int z = 0; z < 16; z++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            int3 local = new int3(x, y, z);
                            int3 world = chunkPosition.ValueRO.Value + local; // Calculate world position from chunk position

                            ushort blockId = GenerateBlockAt(world);// Simple example, replace with actual logic
                            blocks.Add(new Block { BlockId = blockId });
                        }
                    }
                }
            }
        }

        private ushort GenerateBlockAt(int3 worldPosition)
        {

            // need to implement old noise logic and block selection logic here, somehow need to get seed and frequency and other things eurgh. This is going to be a pain.

            return (ushort)(worldPosition.x + worldPosition.y + worldPosition.z);
        }
    }

    public partial struct ChunkMeshingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DirtyChunkTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // This system would handle meshing the chunks based on the blocks in them.
            // It would likely involve generating mesh data and applying it to a MeshRenderer or similar component.
            // For now, this is left as a placeholder for future implementation.
        }
    }

    public partial struct DirtyChunkSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DirtyChunkTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // This system would handle marking chunks as dirty, which would trigger regeneration or meshing.
            // For now, this is left as a placeholder for future implementation.
        }
    }    

    public partial struct ChunkUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChunkTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // This system would handle updating chunks, such as regenerating them or applying changes.
            // For now, this is left as a placeholder for future implementation.
        }
    }
}

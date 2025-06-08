using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Bunker.World
{
    public struct WorldTag : IComponentData { } // Tag component to identify world entities
    public struct Seed : IComponentData
    {
        public int Value; // Seed value for procedural generation
    }

    public class WorldAuthoring : MonoBehaviour
    {
        private class WorldBaker : Baker<WorldAuthoring>
        {
            public override void Bake(WorldAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None); // None because this won't move, doesn't need to be dynamic
                AddComponent(entity, new WorldTag());
                AddComponent(entity, new Seed { Value = 12345 }); // Example seed value, can be set from inspector
            }
        }
    }
    
    public partial struct ChunkSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WorldTag>();
            state.RequireForUpdate<Seed>();
        }

        public void OnUpdate(ref SystemState state)
        {
            EntityManager entityManager = state.EntityManager;

            int3 chunksToGenerate = new int3(4, 4, 4);

            for (int x = 0; x < chunksToGenerate.x; x++)
            {
                for (int y = 0; y < chunksToGenerate.y; y++)
                {
                    for (int z = 0; z < chunksToGenerate.z; z++)
                    {
                        int3 chunkPosition = new int3(x, y, z) * 16;

                        Entity chunkEntity = entityManager.CreateEntity();

                        entityManager.AddComponentData(chunkEntity, new ChunkTag());
                        entityManager.AddComponentData(chunkEntity, new ChunkPosition { Value = chunkPosition });
                        entityManager.AddBuffer<Block>(chunkEntity);
                    }
                }
            }
            state.Enabled = false; // Disable the spawner after generating chunks to prevent continuous generation
        }
    }
}
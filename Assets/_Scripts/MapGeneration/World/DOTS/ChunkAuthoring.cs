using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class ChunkAuthoring : MonoBehaviour
{

    public int3 chunkPosition;

    public class Baker : Baker<ChunkAuthoring>
    {
        public override void Bake(ChunkAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable); // No need to use Dynamic as this is a static chunk, will not move
            AddComponent(entity, new ChunkPosition
            {
                Value = authoring.chunkPosition
            });
        }
    }
}
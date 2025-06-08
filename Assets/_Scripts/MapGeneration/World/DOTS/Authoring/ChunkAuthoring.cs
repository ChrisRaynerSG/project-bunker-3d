using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class ChunkAuthoring : MonoBehaviour
{

    public Vector3Int chunkPosition;
    public int3 chunkPositionInt3 => new int3(chunkPosition.x, chunkPosition.y, chunkPosition.z);
    

    public class Baker : Baker<ChunkAuthoring>
    {
        public override void Bake(ChunkAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable); // No need to use Dynamic as this is a static chunk, will not move
            AddComponent(entity, new ChunkPosition
            {
                Value = authoring.chunkPositionInt3
            });
            AddBuffer<Block>(entity);
        }
    }
}
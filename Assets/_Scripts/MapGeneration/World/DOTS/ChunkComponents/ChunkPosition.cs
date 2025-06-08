using Unity.Entities;
using Unity.Mathematics;

public struct ChunkPosition : IComponentData
{
    public int3 Value;
}
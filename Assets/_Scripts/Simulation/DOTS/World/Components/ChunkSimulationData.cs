using Unity.Entities;
using Unity.Mathematics;

public struct ChunkSimulationData : IComponentData
{
    public int3 chunkPosition;
    public int chunkSize;
    public bool needsUpdate;
}
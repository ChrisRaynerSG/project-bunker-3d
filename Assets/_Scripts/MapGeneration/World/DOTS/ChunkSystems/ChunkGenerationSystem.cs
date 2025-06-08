using Unity.Entities;
using Unity.Burst;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(NoiseGenerationSystem))]
[BurstCompile]
public partial struct ChunkGenerationSystem : ISystem
{
    
}
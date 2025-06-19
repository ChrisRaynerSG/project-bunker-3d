using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct TemperatureSimulationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        
        var job = new TemperatureUpdateJob
        {
            deltaTime = deltaTime
        };
        
        job.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct TemperatureUpdateJob : IJobEntity
{
    public float deltaTime;
    
    public void Execute(ref DynamicBuffer<BlockBuffer> blockBuffer, ref ChunkSimulationData chunkData)
    {
        if (!chunkData.needsUpdate) return;
        
        // Simple temperature diffusion
        for (int i = 0; i < blockBuffer.Length; i++)
        {
            var block = blockBuffer[i];
            
            block.blockData.temperature = math.lerp(block.blockData.temperature, 20f, deltaTime * 0.1f);
            
            blockBuffer[i] = block;
        }
        
        chunkData.needsUpdate = false;
    }
}
// using Unity.Entities;
// using Unity.Burst;
// using Unity.Mathematics;

// [UpdateInGroup(typeof(SimulationSystemGroup))]
// public partial struct TemperatureSimulationSystem : ISystem
// {
//     public void OnCreate(ref SystemState state)
//     {

//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         float deltaTime = SystemAPI.Time.DeltaTime;
//         float airTemperature = 20f;
        
//         var job = new TemperatureUpdateJob
//         {
//             deltaTime = deltaTime
//             , airTemperature = airTemperature   
//         };
        
//         job.ScheduleParallel();
//     }
// }

// [BurstCompile]
// public partial struct TemperatureUpdateJob : IJobEntity
// {
//     public float deltaTime;
//     public float airTemperature; // default air temperature this can be changed based on the current weather time of year etc.
    
//     public void Execute(ref DynamicBuffer<BlockBuffer> blockBuffer, ref ChunkSimulationData chunkData)
//     {
//         if (!chunkData.needsUpdate) return;

//         // Simple temperature diffusion
//         for (int i = 0; i < blockBuffer.Length; i++)
//         {
//             var block = blockBuffer[i];

//             if (block.blockData.blockType == "bunker:air_block")
//             {
//                 block.blockData.temperature = math.lerp(block.blockData.temperature, airTemperature, deltaTime * 0.1f);
//             }
//             else
//             {
//                 block.blockData.temperature = math.lerp(block.blockData.temperature, 20f, deltaTime * 0.1f);
//             }
            

//             blockBuffer[i] = block;
//         }

//         chunkData.needsUpdate = false;
//     }
// }

// disable this system for now, come back to it later as its more of a nice to have than a need to have
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Transforms;
// using Bunker.Entities.Visual;

// [BurstCompile]
// public partial struct CollectTransformDataJob : IJobEntity
// {
//     public NativeArray<GameObjectTransformData> transformData;
    
//     void Execute(
//         [EntityIndexInQuery] int index,
//         in LocalTransform transform, 
//         ref VisualSyncData visualSync)
//     {
//         // Check if position has changed significantly
//         float3 deltaPos = transform.Position - visualSync.lastSyncedPosition;
//         bool positionChanged = math.lengthsq(deltaPos) > 0.001f;
        
//         if (positionChanged || math.any(transform.Rotation.value != visualSync.lastSyncedRotation.value))
//         {
//             transformData[index] = new GameObjectTransformData
//             {
//                 instanceId = visualSync.gameObjectInstanceId,
//                 position = transform.Position,
//                 rotation = transform.Rotation
//             };
            
//             visualSync.lastSyncedPosition = transform.Position;
//             visualSync.lastSyncedRotation = transform.Rotation;
//             visualSync.needsSync = true;
//         }
//         else
//         {
//             visualSync.needsSync = false;
//         }
//     }
// }
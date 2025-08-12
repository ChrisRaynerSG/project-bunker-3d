// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Transforms;
// using UnityEngine;
// using System.Collections.Generic;
// using Bunker.Entities.Visual;


// [UpdateInGroup(typeof(PresentationSystemGroup))]
// public partial class OptimizedGameObjectSyncSystem : SystemBase
// {
//     private EntityQuery _syncQuery;
//     private Dictionary<int, Transform> _gameObjectCache;
//     private List<GameObjectTransformData> _pendingUpdates;

//     protected override void OnCreate()
//     {
//         _syncQuery = GetEntityQuery(typeof(LocalTransform), typeof(VisualSyncData));
//         _gameObjectCache = new Dictionary<int, Transform>();
//         _pendingUpdates = new List<GameObjectTransformData>();
//     }

//     protected override void OnUpdate()
//     {
//         int entityCount = _syncQuery.CalculateEntityCount();
//         if (entityCount == 0) return;

//         // Allocate native arrays for job
//         using var transformData = new NativeArray<GameObjectTransformData>(entityCount, Allocator.TempJob);

//         // Schedule burst-compiled job to collect transform data
//         var collectJob = new CollectTransformDataJob
//         {
//             transformData = transformData
//         };

//         JobHandle jobHandle = collectJob.ScheduleParallel(_syncQuery, Dependency);
//         jobHandle.Complete();

//         // Apply updates to GameObjects on main thread
//         ApplyGameObjectUpdates(transformData);
        
//         Dependency = jobHandle;
//     }

//     private void ApplyGameObjectUpdates(NativeArray<GameObjectTransformData> transformData)
//     {
//         _pendingUpdates.Clear();

//         // Collect updates that actually need to be applied
//         for (int i = 0; i < transformData.Length; i++)
//         {
//             var data = transformData[i];
//             if (data.instanceId != 0) // Valid update
//             {
//                 _pendingUpdates.Add(data);
//             }
//         }

//         // Batch apply updates
//         foreach (var update in _pendingUpdates)
//         {
//             if (_gameObjectCache.TryGetValue(update.instanceId, out Transform transform))
//             {
//                 transform.position = update.position;
//                 transform.rotation = update.rotation;
//             }
//             else
//             {
//                 // Cache miss - find and cache the GameObject
//                 GameObject go = EditorUtility.InstanceIDToObject(update.instanceId) as GameObject;
//                 if (go != null)
//                 {
//                     _gameObjectCache[update.instanceId] = go.transform;
//                     go.transform.position = update.position;
//                     go.transform.rotation = update.rotation;
//                 }
//             }
//         }
//     }

//     protected override void OnDestroy()
//     {
//         _gameObjectCache?.Clear();
//         _pendingUpdates?.Clear();
//     }
// }
// using Unity.Entities;


// [UpdateInGroup(typeof(InitializationSystemGroup))]
// [UpdateBefore(typeof(NoiseGenerationSystem))]
// public partial struct BlockLoadingSystem : ISystem
// {
//     public void OnCreate(ref SystemState state)
//     {
//         var db = BlockDatabase.Instance;


//         var mapSingleton = SystemAPI.ManagedAPI.GetComponent<BlockTypeMapSingleton>(state.SystemHandle);

//         foreach (var block in db.GetAllBlocks())
//         {
//             mapSingleton.BlockTypeMap.Add(block.Value.id, block.Key);
//         }
//         UnityEngine.Debug.Log($"BlockTypeMapSingleton created with {mapSingleton.BlockTypeMap.Count} block types.");
//     }

//     public void OnUpdate(ref SystemState state)
//     {
//         state.Enabled = false;
//     }

//     public void OnDestroy(ref SystemState state)
//     {
//         var mapSingleton = SystemAPI.ManagedAPI.GetComponent<BlockTypeMapSingleton>(state.SystemHandle);
//         mapSingleton.Dispose();
//     }
// }
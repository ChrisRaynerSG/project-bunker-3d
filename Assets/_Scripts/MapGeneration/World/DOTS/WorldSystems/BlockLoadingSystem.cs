using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Rendering;
using Unity.Transforms;

[UpdateBefore(typeof(NoiseGenerationSystem))]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial class BlockLoadingSystem : SystemBase
{
    [BurstCompile]
    protected override void OnCreate()
    {
        RequireForUpdate<WorldTag>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {


        var blobAssetRef = BlockLoader.LoadAndCreateBlobAsset();

        var worldEntity = SystemAPI.GetSingletonEntity<WorldTag>();

        if (!SystemAPI.HasSingleton<BlockDefinitionSingleton>())
        {
            EntityManager.AddComponentData(worldEntity, new BlockDefinitionSingleton
            {
                Blob = blobAssetRef
            });
        }

        UnityEngine.Debug.Log($"BlockDefinitionSingleton created with {blobAssetRef.Value.BlockDefinitions.Length} block types.");
        
        Enabled = false; // Disable this system after the first run
    }
}
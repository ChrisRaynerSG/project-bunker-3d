using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Rendering;


[UpdateAfter(typeof(ChunkBlockGenerationSystem))]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial class ChunkMeshGenerationSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<WorldTag>();
    }

    protected override void OnUpdate()
    {


    }

    NativeList<float3> GenerateVerticesFromBlockData(DynamicBuffer<Block> blocks)
    {
        // This method should generate vertices based on the block data
        // For now, we return an empty array as a placeholder
        return new NativeList<float3>(Allocator.Temp);
    }

    NativeList<int> GenerateTrianglesFromBlockData(DynamicBuffer<Block> blocks)
    {
        // This method should generate triangles based on the block data
        // For now, we return an empty array as a placeholder
        return new NativeList<int>(Allocator.Temp);
    }

    NativeList<float4> GenerateUvsFromBlockData(DynamicBuffer<Block> blocks)
    {
        // This method should generate UVs based on the block data
        // For now, we return an empty array as a placeholder
        return new NativeList<float4>(Allocator.Temp);
    }


}
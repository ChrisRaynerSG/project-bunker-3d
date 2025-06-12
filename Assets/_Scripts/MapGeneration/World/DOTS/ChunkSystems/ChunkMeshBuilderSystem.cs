using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Rendering;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ChunkGenerationSystem))]
public partial struct ChunkMeshBuilderSystem : ISystem
{

    private RenderMeshArray _renderMeshArray;
    private int _nextMeshIndex;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ChunkTag>();
        state.RequireForUpdate<ChunkPosition>();
        state.RequireForUpdate<ChunksGeneratedTag>();
        state.RequireForUpdate<ChunkBlocksInitialisedTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton
        <BeginInitializationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);


        foreach (var (chunkPosition, mesh) in SystemAPI.Query<RefRW<ChunkPosition>, RenderMeshArray>()
            .WithAll<ChunkTag, ChunkBlocksInitialisedTag, DirtyChunkTag>())
        {

        }

    }
    private void BuildMeshForChunk(ChunkPosition chunkPosition)
    {

    }
}



public struct MeshDataDOTS
{
    public NativeList<float3> vertices;
    public NativeList<int> triangles;
    public NativeList<float4> uvs;

    public void Dispose()
    {
        vertices.Dispose();
        triangles.Dispose();
        uvs.Dispose();
    }
}
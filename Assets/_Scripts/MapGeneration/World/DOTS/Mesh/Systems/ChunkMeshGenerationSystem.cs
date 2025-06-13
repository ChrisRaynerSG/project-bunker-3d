using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


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
        // need to go through all the chunks and generate the mesh data for each chunk
        // we do this by looking through the blockBuffer in each chunk and generating the mesh data based on the blocks present
        // we will create faces for each block that is adjacent to air, using ChunkMeshUtilities to create the faces
        // We will also use the EntityCommandBuffer to create the mesh entities and assign the mesh data to them
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Material material = new Material(Shader.Find("Standard"));
        material.mainTexture = TextureAtlasBuilder.AtlasTexture;
        // var material = TextureAtlasBuilder.AtlasTexture;
        var entityManager = World.EntityManager;

        foreach (var (transform, blockbuffer, entity) in SystemAPI.Query<RefRO<LocalTransform>, DynamicBuffer<Block>>()
        .WithEntityAccess()
        .WithAll<ChunkTag, ChunkBlocksInitialisedTag>())
        {
            // Generate mesh data for each chunk
            MeshDataDOTS meshData = new MeshDataDOTS();

            // Iterate through each block in the blockBuffer
            for (int i = 0; i < blockbuffer.Length; i++)
            {
                Block block = blockbuffer[i];
                BlockDefinitionDOTS blockDef = SystemAPI.GetSingleton<BlockDefinitionSingleton>().Blob.Value.BlockDefinitions[block.Id];

                int3 blockPositionInChunk = new int3(
                    i % WorldConstants.CHUNK_SIZE_X,
                    (i / WorldConstants.CHUNK_SIZE_X) % WorldConstants.CHUNK_SIZE_Y,
                    i / (WorldConstants.CHUNK_SIZE_X * WorldConstants.CHUNK_SIZE_Y)
                );

                float3 blockPosition = transform.ValueRO.Position + blockPositionInChunk;

                if (blockDef.IsSolid)
                {
                    ChunkMeshUtilities.CreateFaceUp(meshData, blockPosition, blockDef);
                    ChunkMeshUtilities.CreateFaceDown(meshData, blockPosition, blockDef);
                    ChunkMeshUtilities.CreateFaceNorth(meshData, blockPosition, blockDef);
                    ChunkMeshUtilities.CreateFaceEast(meshData, blockPosition, blockDef);
                    ChunkMeshUtilities.CreateFaceSouth(meshData, blockPosition, blockDef);
                    ChunkMeshUtilities.CreateFaceWest(meshData, blockPosition, blockDef);
                }
                else
                {
                    // If the block is not solid, we can skip generating faces for it
                    continue;
                }
            }

            Mesh mesh = ChunkMeshUtilities.ToMesh(meshData);

            var renderMeshArray = new RenderMeshArray(new Material[] { material }, new Mesh[] { mesh });
            entityManager.SetSharedComponentManaged(entity, renderMeshArray);
            // entityManager.SetComponentData(entity, new MaterialMeshInfo
            // {
            //     Material = renderMeshArray.MaterialReferences[0],
            //     Mesh = renderMeshArray.MeshReferences[0]
            // });
            ecb.SetComponent(entity, new WorldRenderBounds
            {
                Value = mesh.bounds.ToAABB()
            });

            // Create the mesh entity and assign the mesh data
            // Entity meshEntity = ecb.Instantiate(entity);
            // ecb.SetComponent(meshEntity, meshData);
        }
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
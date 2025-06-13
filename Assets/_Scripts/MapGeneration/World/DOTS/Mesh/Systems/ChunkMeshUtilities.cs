using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

using UnityEngine.Rendering;

[BurstCompile]
public partial struct ChunkMeshUtilities
{
    public static void CreateFaceUp(MeshDataDOTS meshData, float3 origin, BlockDefinitionDOTS block)
    {
        float3[] vertices = new float3[4];

        vertices[0] = new float3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[1] = new float3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new float3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new float3(origin.x - .5f, origin.y + .5f, origin.z + .5f);

        using (var nativeVertices = new NativeArray<float3>(vertices, Allocator.Temp))
        {
            meshData.vertices.AddRange(nativeVertices);
        }

        FixedString64Bytes textureName = block.Textures.Top;
        AddTrianglesAndUvs(meshData, vertices, textureName, block);
    }

    public static void CreateFaceDown(MeshDataDOTS meshData, float3 origin, BlockDefinitionDOTS block)
    {
        float3[] vertices = new float3[4];

        vertices[0] = new float3(origin.x - .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new float3(origin.x + .5f, origin.y - .5f, origin.z + .5f);
        vertices[2] = new float3(origin.x + .5f, origin.y - .5f, origin.z - .5f);
        vertices[3] = new float3(origin.x - .5f, origin.y - .5f, origin.z - .5f);

        using (var nativeVertices = new NativeArray<float3>(vertices, Allocator.Temp))
        {
            meshData.vertices.AddRange(nativeVertices);
        }

        FixedString64Bytes textureName = block.Textures.Bottom;
        AddTrianglesAndUvs(meshData, vertices, textureName, block);
    }

    public static void CreateFaceNorth(MeshDataDOTS meshData, float3 origin, BlockDefinitionDOTS block)
    {
        float3[] vertices = new float3[4];

        vertices[0] = new float3(origin.x + .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new float3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new float3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new float3(origin.x - .5f, origin.y - .5f, origin.z + .5f);

        using (var nativeVertices = new NativeArray<float3>(vertices, Allocator.Temp))
        {
            meshData.vertices.AddRange(nativeVertices);
        }

        FixedString64Bytes textureName = block.Textures.Side;
        AddTrianglesAndUvs(meshData, vertices, textureName, block);
    }

    public static void CreateFaceEast(MeshDataDOTS meshData, float3 origin, BlockDefinitionDOTS block)
    {
        float3[] vertices = new float3[4];

        vertices[0] = new float3(origin.x + .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new float3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new float3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new float3(origin.x + .5f, origin.y - .5f, origin.z + .5f);

        using (var nativeVertices = new NativeArray<float3>(vertices, Allocator.Temp))
        {
            meshData.vertices.AddRange(nativeVertices);
        }

        FixedString64Bytes textureName = block.Textures.Side;
        AddTrianglesAndUvs(meshData, vertices, textureName, block);
    }

    public static void CreateFaceSouth(MeshDataDOTS meshData, float3 origin, BlockDefinitionDOTS block)
    {
        float3[] vertices = new float3[4];

        vertices[0] = new float3(origin.x - .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new float3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new float3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new float3(origin.x + .5f, origin.y - .5f, origin.z - .5f);

        using (var nativeVertices = new NativeArray<float3>(vertices, Allocator.Temp))
        {
            meshData.vertices.AddRange(nativeVertices);
        }

        FixedString64Bytes textureName = block.Textures.Side;
        AddTrianglesAndUvs(meshData, vertices, textureName, block);
    }

    public static void CreateFaceWest(MeshDataDOTS meshData, float3 origin, BlockDefinitionDOTS block)
    {
        float3[] vertices = new float3[4];

        vertices[0] = new float3(origin.x - .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new float3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new float3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new float3(origin.x - .5f, origin.y - .5f, origin.z - .5f);

        using (var nativeVertices = new NativeArray<float3>(vertices, Allocator.Temp))
        {
            meshData.vertices.AddRange(nativeVertices);
        }

        FixedString64Bytes textureName = block.Textures.Side;
        AddTrianglesAndUvs(meshData, vertices, textureName, block);
    }

    private static void AddTriangles(MeshDataDOTS meshData, float3[] vertices)
    {

        int baseIndex = meshData.vertices.Length - vertices.Length;

        meshData.triangles.Add(baseIndex);
        meshData.triangles.Add(baseIndex + 1);
        meshData.triangles.Add(baseIndex + 2);
        meshData.triangles.Add(baseIndex);
        meshData.triangles.Add(baseIndex + 2);
        meshData.triangles.Add(baseIndex + 3);
    }

    private static void AddUvs(MeshDataDOTS meshData, FixedString64Bytes textureName, BlockDefinitionDOTS block)
    {
        if (textureName == block.Textures.Top)
            meshData.uvs.Add(block.UvReference.Top);
        else if (textureName == block.Textures.Bottom)
            meshData.uvs.Add(block.UvReference.Bottom);
        else
            meshData.uvs.Add(block.UvReference.Side);
    }
    private static void AddTrianglesAndUvs(MeshDataDOTS meshData, float3[] vertices, FixedString64Bytes textureName, BlockDefinitionDOTS block)
    {
        AddTriangles(meshData, vertices);
        AddUvs(meshData, textureName, block);
    }

    public static Mesh ToMesh(MeshDataDOTS meshData)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(meshData.vertices.ToArray(Allocator.Temp));
        mesh.SetUVs(0, meshData.uvs.ToArray(Allocator.Temp));

        using (NativeArray<int> triangles = meshData.triangles.ToArray(Allocator.Temp))
        {
            int[] triangleArray = new int[triangles.Length];
            triangles.CopyTo(triangleArray);
            mesh.SetTriangles(triangleArray, 0, false);
        }

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }
}
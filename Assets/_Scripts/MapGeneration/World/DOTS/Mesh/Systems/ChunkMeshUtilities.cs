using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Entities;

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
        meshData.uvs.Add(block.UvReference.Top);
    }
    private static void AddTrianglesAndUvs(MeshDataDOTS meshData, float3[] vertices, FixedString64Bytes textureName, BlockDefinitionDOTS block)
    {
        AddTriangles(meshData, vertices);
        AddUvs(meshData, textureName, block);
    }
}
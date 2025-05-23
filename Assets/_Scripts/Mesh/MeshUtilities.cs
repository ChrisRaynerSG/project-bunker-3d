using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshUtilities
{
    public static void CreateFaceUp(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices);
    }

    public static void CreateFaceDown(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices);
    }

    public static void CreateFaceNorth(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices);
    }

    public static void CreateFaceEast(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices);
    }

    public static void CreateFaceSouth(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices);
    }

    public static void CreateFaceWest(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices);
    }

    private static void AddTriangles(MeshData meshData, Vector3[] vertices)
    {
        int[] triangles = new int[6];
        triangles[0] = meshData.vertices.Count - 4;
        triangles[1] = meshData.vertices.Count - 3;
        triangles[2] = meshData.vertices.Count - 2;

        triangles[3] = meshData.vertices.Count - 4;
        triangles[4] = meshData.vertices.Count - 2;
        triangles[5] = meshData.vertices.Count - 1;

        meshData.triangles.AddRange(triangles);
    }

    private static void AddUvs(MeshData meshData)
    {
        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(0, 1);
        uvs[2] = new Vector2(1, 1);
        uvs[3] = new Vector2(1, 0);

        meshData.uvs.AddRange(uvs);
    }

    private static void AddTrianglesAndUvs(MeshData meshData, Vector3[] vertices)
    {
        AddTriangles(meshData, vertices);
        AddUvs(meshData);
    }
}
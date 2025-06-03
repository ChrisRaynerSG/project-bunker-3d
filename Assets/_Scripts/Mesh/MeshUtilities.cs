using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshUtilities
{

    public const int atlasWidth = 4;
    public const int atlasHeight = 2;
    

    public static void CreateFaceUp(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices, tileIndex: 0);
    }

    public static void CreateFaceDown(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices, tileIndex : 0);
    }

    public static void CreateFaceNorth(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices, tileIndex : 0);
    }

    public static void CreateFaceEast(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices, tileIndex : 0);
    }

    public static void CreateFaceSouth(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices, tileIndex : 0);
    }

    public static void CreateFaceWest(MeshData meshData, Vector3 origin)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        AddTrianglesAndUvs(meshData, vertices, tileIndex : 0);
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

    private static void AddUvs(MeshData meshData, int tileIndex, int atlasWidth = 4, int atlasHeight = 2)
    {

        float tileWidth = 1f / atlasWidth;
        float tileHeight = 1f / atlasHeight;

        int x = tileIndex % atlasWidth;
        int y = tileIndex / atlasWidth;

        float uMin = x * tileWidth;
        float vMin = 1f - (y + 1) * tileHeight;


        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(uMin, vMin);
        uvs[1] = new Vector2(uMin, vMin + tileHeight);
        uvs[2] = new Vector2(uMin + tileWidth, vMin + tileHeight);
        uvs[3] = new Vector2(uMin + tileWidth, vMin);

        meshData.uvs.AddRange(uvs);
    }

    private static void AddTrianglesAndUvs(MeshData meshData, Vector3[] vertices, int tileIndex)
    {
        AddTriangles(meshData, vertices);
        AddUvs(meshData, tileIndex);
    }
}
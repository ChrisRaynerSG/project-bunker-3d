using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshUtilities
{

    public const int atlasWidth = 4;
    public const int atlasHeight = 4;
    

    public static void CreateFaceUp(MeshData meshData, Vector3 origin, BlockType type)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        int tileIndex = BlockTextures.BlockUVs[type].GetTextureIndex(BlockTextures.FaceDirection.Up);

        AddTrianglesAndUvs(meshData, vertices, tileIndex);
    }

    public static void CreateFaceDown(MeshData meshData, Vector3 origin, BlockType type)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        int tileIndex = BlockTextures.BlockUVs[type].GetTextureIndex(BlockTextures.FaceDirection.Down);

        AddTrianglesAndUvs(meshData, vertices, tileIndex);
    }

    public static void CreateFaceNorth(MeshData meshData, Vector3 origin, BlockType type)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        int tileIndex = BlockTextures.BlockUVs[type].GetTextureIndex(BlockTextures.FaceDirection.North);

        AddTrianglesAndUvs(meshData, vertices, tileIndex);
    }

    public static void CreateFaceEast(MeshData meshData, Vector3 origin, BlockType type)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        int tileIndex = BlockTextures.BlockUVs[type].GetTextureIndex(BlockTextures.FaceDirection.East);
        AddTrianglesAndUvs(meshData, vertices, tileIndex);
    }

    public static void CreateFaceSouth(MeshData meshData, Vector3 origin, BlockType type)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        int tileIndex = BlockTextures.BlockUVs[type].GetTextureIndex(BlockTextures.FaceDirection.South);

        AddTrianglesAndUvs(meshData, vertices, tileIndex);
    }

    public static void CreateFaceWest(MeshData meshData, Vector3 origin, BlockType type)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        int tileIndex = BlockTextures.BlockUVs[type].GetTextureIndex(BlockTextures.FaceDirection.West);
        AddTrianglesAndUvs(meshData, vertices, tileIndex);
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

    private static void AddUvs(MeshData meshData, int tileIndex)
    {
        float tileWidth = 1f / atlasWidth;
        float tileHeight = 1f / atlasHeight;

        int x = tileIndex % atlasWidth;
        int y = tileIndex / atlasWidth;

        float uMin = x * tileWidth;
        float vMin = 1f - (y + 1) * tileHeight;

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(uMin, vMin + tileHeight);
        uvs[1] = new Vector2(uMin + tileWidth, vMin + tileHeight);
        uvs[2] = new Vector2(uMin + tileWidth, vMin);
        uvs[3] = new Vector2(uMin, vMin);

        meshData.uvs.AddRange(uvs);
    }

    private static void AddTrianglesAndUvs(MeshData meshData, Vector3[] vertices, int tileIndex)
    {
        AddTriangles(meshData, vertices);
        AddUvs(meshData, tileIndex);
    }
}
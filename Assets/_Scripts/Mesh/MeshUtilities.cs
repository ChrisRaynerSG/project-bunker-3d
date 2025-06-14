using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshUtilities
{

    public const int atlasWidth = 4;
    public const int atlasHeight = 4;


    public static void CreateFaceUp(MeshData meshData, Vector3 origin, BlockDefinition block)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        string textureName = block.textures.top;

        AddTrianglesAndUvs(meshData, vertices, textureName);
    }

    public static void CreateFaceDown(MeshData meshData, Vector3 origin, BlockDefinition block)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        string textureName = block.textures.bottom;

        AddTrianglesAndUvs(meshData, vertices, textureName);
    }

    public static void CreateFaceNorth(MeshData meshData, Vector3 origin, BlockDefinition block)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        string textureName = block.textures.side;

        AddTrianglesAndUvs(meshData, vertices, textureName);
    }

    public static void CreateFaceEast(MeshData meshData, Vector3 origin, BlockDefinition block)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z + .5f);
        vertices[3] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z + .5f);

        meshData.vertices.AddRange(vertices);

        string textureName = block.textures.side;
        AddTrianglesAndUvs(meshData, vertices, textureName);
    }

    public static void CreateFaceSouth(MeshData meshData, Vector3 origin, BlockDefinition block)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);
        vertices[1] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[2] = new Vector3(origin.x + .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x + .5f, origin.y - .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        string textureName = block.textures.side;

        AddTrianglesAndUvs(meshData, vertices, textureName);
    }

    public static void CreateFaceWest(MeshData meshData, Vector3 origin, BlockDefinition block)
    {
        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z + .5f);
        vertices[1] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z + .5f);
        vertices[2] = new Vector3(origin.x - .5f, origin.y + .5f, origin.z - .5f);
        vertices[3] = new Vector3(origin.x - .5f, origin.y - .5f, origin.z - .5f);

        meshData.vertices.AddRange(vertices);

        string textureName = block.textures.side;
        AddTrianglesAndUvs(meshData, vertices, textureName);
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

    private static void AddUvs(MeshData meshData, string textureName)
    {
        if (!TextureAtlasBuilder.UVRects.TryGetValue(textureName, out Rect uvRect))
        {
            Debug.LogWarning($"Texture {textureName} not found in UVRects.");
            return;
        }

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(uvRect.xMin, uvRect.yMin);
        uvs[1] = new Vector2(uvRect.xMin, uvRect.yMax);
        uvs[2] = new Vector2(uvRect.xMax, uvRect.yMax);
        uvs[3] = new Vector2(uvRect.xMax, uvRect.yMin);

        meshData.uvs.AddRange(uvs);
    }

    private static void AddTrianglesAndUvs(MeshData meshData, Vector3[] vertices, string textureName)
    {
        AddTriangles(meshData, vertices);
        AddUvs(meshData, textureName);
    }

        public static void LoadMeshData(MeshData meshData, MeshFilter filter)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = meshData.vertices.ToArray();
        mesh.triangles = meshData.triangles.ToArray();
        mesh.uv = meshData.uvs.ToArray();

        mesh.RecalculateNormals();

        filter.mesh = mesh;
    }
}
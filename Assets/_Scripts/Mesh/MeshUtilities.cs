using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Provides static utility methods for procedural mesh generation of block-based worlds.
/// 
/// <see cref="MeshUtilities"/> includes methods for creating individual block faces, adding triangles and UVs,
/// and assembling complete meshes for use with Unity's MeshFilter. It also supports texture atlas UV mapping.
/// </summary>
public static class MeshUtilities
{
    /// <summary>
    /// Adds the vertices, triangles, and UVs for the top (upward-facing) face of a block.
    /// </summary>
    /// <param name="meshData">The mesh data to append to.</param>
    /// <param name="origin">The center position of the block.</param>
    /// <param name="block">The block definition for texture selection.</param>
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

    /// <summary>
    /// Adds the vertices, triangles, and UVs for the bottom (downward-facing) face of a block.
    /// </summary>
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

    /// <summary>
    /// Adds the vertices, triangles, and UVs for the north (positive Z) face of a block.
    /// </summary>
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

    /// <summary>
    /// Adds the vertices, triangles, and UVs for the east (positive X) face of a block.
    /// </summary>
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

    /// <summary>
    /// Adds the vertices, triangles, and UVs for the south (negative Z) face of a block.
    /// </summary>
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

    /// <summary>
    /// Adds the vertices, triangles, and UVs for the west (negative X) face of a block.
    /// </summary>
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

    /// <summary>
    /// Adds triangles to the mesh data for the last four added vertices.
    /// </summary>
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

    /// <summary>
    /// Adds UV coordinates to the mesh data for the last four added vertices, using the texture atlas.
    /// </summary>
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

    /// <summary>
    /// Adds triangles and UVs for the last four added vertices.
    /// </summary>
    private static void AddTrianglesAndUvs(MeshData meshData, Vector3[] vertices, string textureName)
    {
        AddTriangles(meshData, vertices);
        AddUvs(meshData, textureName);
    }

    /// <summary>
    /// Loads the mesh data into a Unity <see cref="MeshFilter"/>, creating a new mesh.
    /// </summary>
    /// <param name="meshData">The mesh data to load.</param>
    /// <param name="filter">The mesh filter to assign the mesh to.</param>
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

    /// <summary>
    /// Creates all visible faces for a block at the given position, based on neighbor solidity.
    /// </summary>
    /// <param name="y">The Y coordinate of the block.</param>
    /// <param name="meshData">The mesh data to append to.</param>
    /// <param name="worldX">The world X coordinate of the block.</param>
    /// <param name="worldZ">The world Z coordinate of the block.</param>
    /// <param name="targetPosition">The local position of the block.</param>
    /// <param name="blockData">The block data for face/texture selection.</param>
    /// <param name="forceTop">If true, always creates the top face regardless of neighbors.</param>
    public static void CreateFaces(int y, MeshData meshData, int worldX, int worldZ, Vector3 targetPosition, BlockData blockData, bool forceTop = false)
    {
        if (forceTop)
        {
            CreateFaceUp(meshData, targetPosition, blockData.definition);
        }
        else
        {
            if (y == World.Instance.currentElevation)
            {
                CreateFaceUp(meshData, targetPosition, blockData.definition);
            }
            else
            {
                if (!BlockUtils.IsSolid(worldX, y + 1, worldZ)) CreateFaceUp(meshData, targetPosition, blockData.definition);
            }
        }
        if (!BlockUtils.IsSolid(worldX, y - 1, worldZ)) CreateFaceDown(meshData, targetPosition, blockData.definition);
        if (!BlockUtils.IsSolid(worldX, y, worldZ + 1)) CreateFaceNorth(meshData, targetPosition, blockData.definition);
        if (!BlockUtils.IsSolid(worldX + 1, y, worldZ)) CreateFaceEast(meshData, targetPosition, blockData.definition);
        if (!BlockUtils.IsSolid(worldX, y, worldZ - 1)) CreateFaceSouth(meshData, targetPosition, blockData.definition);
        if (!BlockUtils.IsSolid(worldX - 1, y, worldZ)) CreateFaceWest(meshData, targetPosition, blockData.definition);
    }

    public static void AddBlocksToMesh(
        BlockAccessor blockAccessor,
        List<Vector3Int> positions,
        Vector3 basePosition,
        MeshData meshData)
    {
        foreach (var pos in positions)
        {
            BlockData blockData = blockAccessor.GetBlockDataFromPosition(pos);
            Vector3 localPos = pos - basePosition;
            MeshUtilities.CreateFaces(pos.y, meshData, pos.x, pos.z, localPos, blockData, true);
        }
    }

}
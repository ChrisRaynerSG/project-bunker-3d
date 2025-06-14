using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores mesh construction data for a chunk or block mesh.
/// 
/// <see cref="MeshData"/> holds the lists of vertices, triangles, and UVs that are used to build a Unity mesh.
/// This structure is typically filled during mesh generation and then converted to a <see cref="Mesh"/>.
/// </summary>
public class MeshData
{
    /// <summary>
    /// The list of vertex positions for the mesh.
    /// </summary>
    public List<Vector3> vertices = new List<Vector3>();

    /// <summary>
    /// The list of triangle indices for the mesh.
    /// </summary>
    public List<int> triangles = new List<int>();

    /// <summary>
    /// The list of UV coordinates for the mesh.
    /// </summary>
    public List<Vector2> uvs = new List<Vector2>();
}
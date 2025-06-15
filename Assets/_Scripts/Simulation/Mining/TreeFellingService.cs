using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the logic for felling (cutting down) trees in the game world.
/// 
/// When a tree block is mined, this service is responsible for:
/// - Identifying the tree at the given position.
/// - Removing the mined block and updating the tree data.
/// - Collecting all log blocks above the mined position (for falling effect).
/// - Collecting all leaf blocks associated with the tree.
/// - Instantiating a falling tree prefab with a generated mesh for the logs and leaves above.
/// - Removing all affected log and leaf blocks from the world.
/// - Removing the tree from world data if no logs remain.
/// 
/// Dependencies:
/// - <see cref="BlockAccessor"/>: For reading and modifying block data in the world.
/// - <see cref="BlockDatabase"/>: For accessing block definitions and textures.
/// - <see cref="GameObject"/> fallingTreePrefab: Prefab used for the falling tree effect.
/// </summary>
public class TreeFellingService
{
    private BlockAccessor blockAccessor;
    private BlockDatabase blockDatabase;
    private GameObject fallingTreePrefab;

    /// <summary>
    /// Constructs a new TreeFellingService.
    /// </summary>
    /// <param name="accessor">Block accessor for world manipulation.</param>
    /// <param name="db">Block database for block definitions and textures.</param>
    /// <param name="prefab">Prefab for the falling tree effect.</param>
    public TreeFellingService(BlockAccessor accessor, BlockDatabase db, GameObject prefab)
    {
        blockAccessor = accessor;
        blockDatabase = db;
        fallingTreePrefab = prefab;
    }

    /// <summary>
    /// Attempts to fell (cut down) the tree at the specified block position.
    /// If a tree is found, removes the block, spawns a falling tree, and updates world data.
    /// </summary>
    /// <param name="hitPosition">The position of the mined block.</param>
    public void FellTreeAt(Vector3Int hitPosition)
    {
        TreeGameData tree = TreeUtilities.FindTreeAtPosition(hitPosition);
        if (tree == null) return;

        RemoveBlock(hitPosition, tree);

        List<Vector3Int> logsAbove = CollectLogsAbove(tree, hitPosition);
        List<Vector3Int> leavesAbove = new List<Vector3Int>(tree.LeafPositions);

        if (logsAbove.Count > 0)
        {
            InstantiateFallingTree(logsAbove, leavesAbove);
        }

        RemoveLogsAbove(tree, hitPosition);
        RemoveLeaves(tree);

        if (tree.logBlocks.Count == 0)
            WorldData.Instance.RemoveTree(tree);
    }

    /// <summary>
    /// Removes a single block from the world and updates the tree data.
    /// </summary>
    private void RemoveBlock(Vector3Int position, TreeGameData tree)
    {
        blockAccessor.SetBlock(position, blockAccessor.GetBlockDef("bunker:air_block"));
        tree.RemoveLogBlock(blockAccessor.GetBlockDataFromPosition(position));
    }

    /// <summary>
    /// Collects all log blocks above the given position in the same XZ column.
    /// </summary>
    private List<Vector3Int> CollectLogsAbove(TreeGameData tree, Vector3Int hitPosition)
    {
        List<Vector3Int> logsAbove = new List<Vector3Int>();
        foreach (var pos in tree.LogPositions)
        {
            if (pos.x == hitPosition.x && pos.z == hitPosition.z && pos.y > hitPosition.y)
                logsAbove.Add(pos);
        }
        logsAbove.Sort((a, b) => a.y.CompareTo(b.y));
        return logsAbove;
    }

    /// <summary>
    /// Instantiates the falling tree prefab and generates a mesh for the logs and leaves above the cut.
    /// Applies physics to simulate the tree falling.
    /// </summary>
    private void InstantiateFallingTree(List<Vector3Int> logsAbove, List<Vector3Int> leavesAbove)
    {
        Vector3 basePosition = logsAbove[0];
        GameObject fallingTree = Object.Instantiate(fallingTreePrefab, basePosition, Quaternion.identity);
        Object.Destroy(fallingTree, 10f);

        MeshData meshData = new MeshData();
        MeshUtilities.AddBlocksToMesh(blockAccessor, logsAbove, basePosition, meshData);
        MeshUtilities.AddBlocksToMesh(blockAccessor, leavesAbove, basePosition, meshData);

        Mesh mesh = new Mesh();
        mesh.vertices = meshData.vertices.ToArray();
        mesh.triangles = meshData.triangles.ToArray();
        mesh.uv = meshData.uvs.ToArray();
        mesh.RecalculateNormals();

        MeshRenderer renderer = fallingTree.GetComponent<MeshRenderer>();
        renderer.material.mainTexture = blockDatabase.TextureAtlas;

        Rigidbody rb = fallingTree.GetComponent<Rigidbody>();
        fallingTree.GetComponent<MeshFilter>().mesh = mesh;
        fallingTree.GetComponent<MeshCollider>().sharedMesh = mesh;
        fallingTree.GetComponent<MeshCollider>().convex = true;
        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 tipDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        float pushStrength = Random.Range(1f, 3f);
        float randomTorque = Random.Range(2f, 5f);
        rb.AddTorque(Vector3.Cross(Vector3.up, tipDirection) * randomTorque, ForceMode.Impulse);
        rb.AddForce(tipDirection * pushStrength, ForceMode.Impulse);
    }

    /// <summary>
    /// Removes all log blocks at or above the given Y position from the world and tree data.
    /// </summary>
    private void RemoveLogsAbove(TreeGameData tree, Vector3Int hitPosition)
    {
        List<Vector3Int> logsToRemove = new List<Vector3Int>();
        foreach (var logPosition in tree.LogPositions)
        {
            if (logPosition.y >= hitPosition.y)
                logsToRemove.Add(logPosition);
        }
        foreach (var logPosition in logsToRemove)
        {
            blockAccessor.SetBlock(logPosition, blockAccessor.GetBlockDef("bunker:air_block"));
            BlockData removedLogBlock = blockAccessor.GetBlockDataFromPosition(logPosition);
            tree.RemoveLogBlock(removedLogBlock);
        }
    }

    /// <summary>
    /// Removes all leaf blocks associated with the tree from the world.
    /// </summary>
    private void RemoveLeaves(TreeGameData tree)
    {
        List<Vector3Int> leavesToRemove = new List<Vector3Int>(tree.LeafPositions);
        foreach (var leafPosition in leavesToRemove)
        {
            blockAccessor.SetBlock(leafPosition, blockAccessor.GetBlockDef("bunker:air_block"));
        }
    }
}
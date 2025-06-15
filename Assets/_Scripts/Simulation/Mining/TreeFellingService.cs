using System.Linq;
using UnityEngine;

public class TreeFellingService
{
    private BlockAccessor blockAccessor;
    private BlockDatabase blockDatabase;
    private GameObject fallingTreePrefab;

    public TreeFellingService(BlockAccessor accessor, BlockDatabase db, GameObject prefab)
    {
        blockAccessor = accessor;
        blockDatabase = db;
        fallingTreePrefab = prefab;
    }

    public void FellTreeAt(Vector3Int hitPosition)
    {
        TreeGameData tree = WorldData.Instance.Trees.Find(t => t.LogPositions.Contains(hitPosition));
        if (tree == null) return;

        blockAccessor.SetBlock(hitPosition, blockAccessor.GetBlockDef("bunker:air_block"));
        tree.RemoveLogBlock(blockAccessor.GetBlockDataFromPosition(hitPosition));

        var logsAbove = tree.LogPositions
            .Where(pos => pos.x == hitPosition.x && pos.z == hitPosition.z && pos.y > hitPosition.y)
            .OrderBy(pos => pos.y)
            .ToList();

        var leavesAbove = tree.LeafPositions.ToList();

        if (logsAbove.Count > 0)
        {
            Vector3 basePosition = logsAbove[0];
            GameObject fallingTree = Object.Instantiate(fallingTreePrefab, basePosition, Quaternion.identity);
            Object.Destroy(fallingTree, 10f);

            MeshData meshData = new MeshData();
            foreach (var logPos in logsAbove)
            {
                BlockData blockData = blockAccessor.GetBlockDataFromPosition(logPos);
                Vector3 localPos = logPos - basePosition;
                MeshUtilities.CreateFaces(logPos.y, meshData, logPos.x, logPos.z, localPos, blockData, true);
            }
            foreach (var leafPos in leavesAbove)
            {
                BlockData blockData = blockAccessor.GetBlockDataFromPosition(leafPos);
                Vector3 localPos = leafPos - basePosition;
                MeshUtilities.CreateFaces(leafPos.y, meshData, leafPos.x, leafPos.z, localPos, blockData, true);
            }

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

        foreach (var logPosition in tree.LogPositions.Where(lp => lp.y >= hitPosition.y).ToList())
        {
            blockAccessor.SetBlock(logPosition, blockAccessor.GetBlockDef("bunker:air_block"));
            BlockData removedLogBlock = blockAccessor.GetBlockDataFromPosition(logPosition);
            tree.RemoveLogBlock(removedLogBlock);
        }
        foreach (var leafPosition in tree.LeafPositions.ToList())
        {
            blockAccessor.SetBlock(leafPosition, blockAccessor.GetBlockDef("bunker:air_block"));
        }
        if (tree.logBlocks.Count == 0) WorldData.Instance.RemoveTree(tree);
    }
}
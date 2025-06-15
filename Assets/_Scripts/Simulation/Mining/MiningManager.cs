using System.Linq;
using UnityEngine;

public class MiningManager : MonoBehaviour
{
    public Camera mainCamera;

    public GameObject FallingTreePrefab;

    private BlockAccessor blockAccessor;

    private BlockDatabase blockDatabase;


    void Start()
    {
        blockAccessor = new BlockAccessor(World.Instance);
        blockDatabase = BlockDatabase.Instance;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryMineBlock();
        }
    }

    private void TryMineBlock()
    {
        Debug.Log("Mining block at mouse position...");
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        int layerMask = LayerMask.GetMask("Default");

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, layerMask))
        {
            Debug.Log($"Hit block at position: {hit.point}, normal: {hit.normal}");
            Vector3 hitOffset = hit.point - hit.normal * 0.5f;
            Vector3Int hitPosition = Vector3Int.RoundToInt(hitOffset);
            BlockData hitBlockData = blockAccessor.GetBlockDataFromPosition(hitPosition);
            Debug.Log($"Hit block data: {hitBlockData.definition.id} at position {hitPosition}");
            if (blockAccessor.GetBlockDataFromPosition(hitPosition).definition.id == "bunker:oak_tree_log_block")
            {
                TreeGameData tree = WorldData.Instance.Trees.Find(t => t.LogPositions.Contains(hitPosition));
                if (tree != null)
                {
                    blockAccessor.SetBlock(hitPosition, blockAccessor.GetBlockDef("bunker:air_block"));
                    tree.RemoveLogBlock(blockAccessor.GetBlockDataFromPosition(hitPosition));

                    var logsAbove = tree.LogPositions
                        .Where(pos => pos.x == hitPosition.x && pos.z == hitPosition.z && pos.y > hitPosition.y)
                        .OrderBy(pos => pos.y)
                        .ToList();

                    var leavesAbove = tree.LeafPositions.ToList();

                    if (logsAbove.Count > 0)
                    {
                        // 2. Instantiate the prefab at the base position
                        Vector3 basePosition = logsAbove[0];
                        GameObject fallingTree = Instantiate(FallingTreePrefab, basePosition, Quaternion.identity);

                        // 3. Generate mesh for the logs above
                        MeshData meshData = new MeshData();

                        foreach (var logPos in logsAbove)
                        {

                            BlockData blockData = blockAccessor.GetBlockDataFromPosition(logPos);
                            Vector3 localPos = logPos - basePosition; // Local to prefab
                            MeshUtilities.CreateFaces(logPos.y, meshData, logPos.x, logPos.z, localPos, blockData, true);
                        }

                        foreach (var leafPos in leavesAbove)
                        {
                            BlockData blockData = blockAccessor.GetBlockDataFromPosition(leafPos);
                            Vector3 localPos = leafPos - basePosition; // Local to prefab
                            MeshUtilities.CreateFaces(leafPos.y, meshData, leafPos.x, leafPos.z, localPos, blockData, true);
                        }

                        Mesh mesh = new Mesh();
                        mesh.vertices = meshData.vertices.ToArray();
                        mesh.triangles = meshData.triangles.ToArray();
                        mesh.uv = meshData.uvs.ToArray();
                        mesh.RecalculateNormals();

                        MeshRenderer renderer = fallingTree.GetComponent<MeshRenderer>();
                        renderer.material.mainTexture = blockDatabase.TextureAtlas;

                        fallingTree.GetComponent<MeshFilter>().mesh = mesh;
                        fallingTree.GetComponent<MeshCollider>().sharedMesh = mesh;
                        fallingTree.GetComponent<MeshCollider>().convex = true;
                        fallingTree.GetComponent<Rigidbody>().isKinematic = false;
                        fallingTree.GetComponent<Rigidbody>().useGravity = true;
                        // fallingTree.GetComponent<Rigidbody>().AddForce(Vector3.down * 10f, ForceMode.Impulse);
                    }

                    foreach (var logPosition in tree.LogPositions)
                    {
                        if (logPosition.y >= hitPosition.y)
                        {
                            blockAccessor.SetBlock(logPosition, blockAccessor.GetBlockDef("bunker:air_block"));
                            BlockData removedLogBlock = blockAccessor.GetBlockDataFromPosition(logPosition);
                            tree.RemoveLogBlock(removedLogBlock);
                        }
                    }
                    foreach (var leafPosition in tree.LeafPositions)
                    {
                        blockAccessor.SetBlock(leafPosition, blockAccessor.GetBlockDef("bunker:air_block"));
                    }
                    if (tree.logBlocks.Count == 0) WorldData.Instance.RemoveTree(tree);
                }
            }
            else
            {
                blockAccessor.SetBlock(hitPosition, blockAccessor.GetBlockDef("bunker:air_block"));
            }
        }
    }
}
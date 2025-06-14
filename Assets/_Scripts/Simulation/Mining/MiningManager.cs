using UnityEngine;

public class MiningManager : MonoBehaviour
{
    public Camera mainCamera;

    private BlockAccessor blockAccessor;


    void Start()
    {
        blockAccessor = new BlockAccessor(World.Instance);
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
            if (blockAccessor.GetBlockDataFromPosition(hitPosition).definition.id == "bunker:oak_tree_log_block")
            {
                TreeGameData tree = WorldData.Instance.Trees.Find(t => t.LogPositions.Contains(hitPosition));
                if (tree != null)
                {
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
                    if(tree.logBlocks.Count == 0) WorldData.Instance.RemoveTree(tree);
                }
            }
            else
            {
                blockAccessor.SetBlock(hitPosition, blockAccessor.GetBlockDef("bunker:air_block"));
            }
        }
    }
}
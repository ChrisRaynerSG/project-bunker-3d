using System.Linq;
using UnityEngine;

public class MiningManager : MonoBehaviour
{
    public Camera mainCamera;

    public GameObject FallingTreePrefab;

    private BlockAccessor blockAccessor;

    private BlockDatabase blockDatabase;

    private TreeFellingService treeFellingService;


    void Start()
    {
        blockAccessor = new BlockAccessor(World.Instance);
        blockDatabase = BlockDatabase.Instance;
        treeFellingService = new TreeFellingService(blockAccessor, blockDatabase, FallingTreePrefab);
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
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("Default");
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, layerMask))
        {
            Vector3 hitOffset = hit.point - hit.normal * 0.5f;
            Vector3Int hitPosition = Vector3Int.RoundToInt(hitOffset);

            if (blockAccessor.GetBlockDataFromPosition(hitPosition).definition.id == "bunker:oak_tree_log_block")
            {
                treeFellingService.FellTreeAt(hitPosition);
            }
            else
            {
                blockAccessor.SetBlock(hitPosition, blockAccessor.GetBlockDef("bunker:air_block"));
            }
        }
    }
}
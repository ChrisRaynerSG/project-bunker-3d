using System.Linq;
using UnityEngine;

public class MiningManager : MonoBehaviour
{
    public Camera mainCamera;

    public GameObject FallingTreePrefab;

    private BlockAccessor blockAccessor;

    private BlockDatabase blockDatabase;

    private TreeFellingService treeFellingService;

    private Vector3Int position1;
    private Vector3Int position2;
    private bool isSelectingArea = false;

    void Start()
    {
        blockAccessor = new BlockAccessor(World.Instance);
        blockDatabase = BlockDatabase.Instance;
        treeFellingService = new TreeFellingService(blockAccessor, blockDatabase, FallingTreePrefab);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isSelectingArea)
        {
            position1 = GetHitPosition();
            isSelectingArea = true;
        }
        else if( Input.GetMouseButtonDown(1) && isSelectingArea)
        {
            // Cancel selection
            isSelectingArea = false;
            position1 = Vector3Int.zero;
            position2 = Vector3Int.zero;
        }
        else if (Input.GetMouseButtonDown(0) && isSelectingArea)
        {
            position2 = GetHitPosition();
            isSelectingArea = false;

            // Calculate the area to mine
            Vector3Int minPos = Vector3Int.Min(position1, position2);
            Vector3Int maxPos = Vector3Int.Max(position1, position2);

            for (int x = minPos.x; x <= maxPos.x; x++)
            {
                for (int y = minPos.y; y <= maxPos.y; y++)
                {
                    for (int z = minPos.z; z <= maxPos.z; z++)
                    {
                        TryMineBlock(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }

    private void TryMineBlock(Vector3Int hitPosition)
    {
        if(hitPosition == Vector3Int.zero) return;
        
        if (blockAccessor.GetBlockDataFromPosition(hitPosition).definition.id == "bunker:oak_tree_log_block")
        {
            treeFellingService.FellTreeAt(hitPosition);
        }
        else
        {
            blockAccessor.SetBlock(hitPosition, blockAccessor.GetBlockDef("bunker:air_block"));
        }
    }
    private Vector3Int GetHitPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("Default");
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, layerMask))
        {
            Vector3 hitOffset = hit.point - hit.normal * 0.5f;
            return Vector3Int.RoundToInt(hitOffset);
        }
        return Vector3Int.zero;
    }
}
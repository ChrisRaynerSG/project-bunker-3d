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
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"Hit block at position: {hit.point}, normal: {hit.normal}");
            Vector3Int hitPosition = Vector3Int.FloorToInt(hit.point - hit.normal * 0.01f);
            blockAccessor.SetBlock(hitPosition, BlockData.BlockType.Air);
        }
    }
}
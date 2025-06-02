using UnityEngine;

public class BlockPlacementManager : MonoBehaviour
{
    public Camera mainCamera;
    private BlockAccessor blockAccessor;

    void Start()
    {
        blockAccessor = new BlockAccessor(World.Instance);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            TryPlaceBlock();
        }
    }

    private void TryPlaceBlock()
    {
        Debug.Log("Placing block at mouse position...");
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        int layerMask = LayerMask.GetMask("Default");

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, layerMask))
        {
            Debug.Log($"Hit position: {hit.point}, normal: {hit.normal}");
            Vector3 hitOffset = hit.point + hit.normal * 0.5f; // Offset to place block on the surface
            Vector3Int hitPosition = Vector3Int.RoundToInt(hitOffset);

            // Place a new block at the hit position
            blockAccessor.SetBlock(hitPosition, BlockData.BlockType.Dirt);
        }
    }
}
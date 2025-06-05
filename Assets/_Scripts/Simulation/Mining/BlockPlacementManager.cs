using UnityEngine;

public class BlockPlacementManager : MonoBehaviour
{
    public Camera mainCamera;
    private BlockAccessor blockAccessor;

    Vector3 clickDownPosition;
    Vector3 clickUpPosition;


    void Start()
    {
        blockAccessor = new BlockAccessor(World.Instance);
    }

    void Update()
    {
        HandleInput();
    }

    private void TryPlaceBlock()
    {
        Debug.Log("Placing block at mouse position...");
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        int layerMask = LayerMask.GetMask("Default");

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, layerMask))
        {

            //what happens if we try to place a block outside the world bounds? 
            // at the moment it sometimes randomly places the block in the world in the same chunk at the same y level

            Debug.Log($"Hit position: {hit.point}, normal: {hit.normal}");
            Vector3 hitOffset = hit.point + hit.normal * 0.5f; // Offset to place block on the surface
            Vector3Int hitPosition = Vector3Int.RoundToInt(hitOffset);

            // Place a new block at the hit position
            blockAccessor.SetBlock(hitPosition,World.Instance.GetBlockDefinitions()["bunker:stone_block"]);
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button to place block
        {
            clickDownPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            clickUpPosition = Input.mousePosition;

            if (Vector3.Distance(clickDownPosition, clickUpPosition) < 0.1f) // Check if the click was a short press
            {
                TryPlaceBlock();
            }
            else
            {
                return;
            }
        }
    }
}
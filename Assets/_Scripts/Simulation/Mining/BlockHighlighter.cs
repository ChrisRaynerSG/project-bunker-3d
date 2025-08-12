using UnityEngine;

public class BlockHighlighter : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject highlightCube;
    private Renderer _highlightRenderer;
    private bool _lastVisibleState = false;
    private bool isHighlightingActive = false;
    private BlockAccessor _blockAccessor;
    public static event System.Action<BlockData> OnBlockHovered;

    void Start()
    {
        _blockAccessor = new BlockAccessor(World.Instance);
        _highlightRenderer = highlightCube.GetComponent<Renderer>();
        if (_highlightRenderer == null)
        {
            Debug.LogError("Highlight cube does not have a Renderer component.");
            return;
        }
        _highlightRenderer.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            isHighlightingActive = !isHighlightingActive;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("Default");

        bool shouldBeVisible = false;

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, layerMask))
        {
            Vector3 hitOffset = hit.point - hit.normal * 0.5f;
            Vector3Int hitPosition = Vector3Int.RoundToInt(hitOffset);
            highlightCube.transform.position = hitPosition;

            OnBlockHovered?.Invoke(_blockAccessor.GetBlockDataFromPosition(hitPosition));
            shouldBeVisible = isHighlightingActive;
        }

        if (shouldBeVisible != _lastVisibleState)
        {
            _highlightRenderer.enabled = shouldBeVisible;
            _lastVisibleState = shouldBeVisible;
        }
    }
}
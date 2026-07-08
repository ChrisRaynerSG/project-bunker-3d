using UnityEngine;
using UnityEngine.Rendering;

public class MiningManager : MonoBehaviour, IUpdatable
{
    private const float SelectionPreviewPadding = 0.02f;

    public Camera mainCamera;

    public GameObject FallingTreePrefab;

    private BlockAccessor _blockAccessor;

    private BlockDatabase _blockDatabase;

    private TreeFellingService _treeFellingService;

    private bool _isMining = false;

    private Vector3Int _position1;
    private Vector3Int _position2;
    private bool _isSelectingArea = false;
    private GameObject _selectionPreview;
    private MeshRenderer _selectionPreviewRenderer;
    private Material _selectionPreviewMaterial;

    void Start()
    {
        _blockAccessor = new BlockAccessor(World.Instance);
        _blockDatabase = BlockDatabase.Instance;
        _treeFellingService = new TreeFellingService(_blockAccessor, _blockDatabase, FallingTreePrefab);

        // Hand the shared world-modification services to the job system so that a
        // dweller can actually remove the block (or fell the tree) when it finishes
        // a mining job.
        JobManager.Instance.Configure(_blockAccessor, _blockDatabase, _treeFellingService);

        CreateSelectionPreview();
    }

    void OnEnable()
    {
        UpdateManager.Register(this);
    }

    void OnDisable()
    {
        UpdateManager.Unregister(this);
    }

    public void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0) && !_isSelectingArea)
        {
            if (TryGetHitPosition(out _position1))
            {
                _isSelectingArea = true;
                UpdateSelectionPreview(_position1);
            }
        }
        else if( Input.GetMouseButtonDown(1) && _isSelectingArea)
        {
            ClearSelection();
        }
        else if (Input.GetMouseButtonDown(0) && _isSelectingArea)
        {
            if (TryGetHitPosition(out _position2))
            {
                Vector3Int minPos = Vector3Int.Min(_position1, _position2);
                Vector3Int maxPos = Vector3Int.Max(_position1, _position2);

                for (int x = minPos.x; x <= maxPos.x; x++)
                {
                    for (int y = minPos.y; y <= maxPos.y; y++)
                    {
                        for (int z = minPos.z; z <= maxPos.z; z++)
                        {
                            // Instead of removing the block instantly, queue a mining
                            // job. A dweller will path to it and carry out the work.
                            JobManager.Instance.EnqueueMiningJob(new Vector3Int(x, y, z));
                        }
                    }
                }

                ClearSelection();
            }
        }

        if (_isSelectingArea)
        {
            if (TryGetHitPosition(out Vector3Int hoveredPosition))
            {
                UpdateSelectionPreview(hoveredPosition);
            }
            else
            {
                HideSelectionPreview();
            }
        }
    }

    private bool TryGetHitPosition(out Vector3Int hitPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int layerMask = LayerMask.GetMask("Default");
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, layerMask))
        {
            Vector3 hitOffset = hit.point - hit.normal * 0.5f;
            hitPosition = Vector3Int.RoundToInt(hitOffset);
            return true;
        }

        hitPosition = default;
        return false;
    }

    private void CreateSelectionPreview()
    {
        _selectionPreview = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _selectionPreview.name = "MiningSelectionPreview";
        _selectionPreview.transform.SetParent(transform, false);
        _selectionPreview.layer = LayerMask.NameToLayer("Ignore Raycast");

        Collider previewCollider = _selectionPreview.GetComponent<Collider>();
        if (previewCollider != null)
        {
            Destroy(previewCollider);
        }

        _selectionPreviewRenderer = _selectionPreview.GetComponent<MeshRenderer>();
        _selectionPreviewMaterial = CreateSelectionPreviewMaterial();
        if (_selectionPreviewRenderer != null && _selectionPreviewMaterial != null)
        {
            _selectionPreviewRenderer.sharedMaterial = _selectionPreviewMaterial;
            _selectionPreviewRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _selectionPreviewRenderer.receiveShadows = false;
            _selectionPreviewRenderer.lightProbeUsage = LightProbeUsage.Off;
            _selectionPreviewRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            _selectionPreviewRenderer.enabled = false;
        }
    }

    private Material CreateSelectionPreviewMaterial()
    {
        Shader previewShader =
            Shader.Find("Universal Render Pipeline/Unlit") ??
            Shader.Find("Unlit/Color") ??
            Shader.Find("Sprites/Default");

        if (previewShader == null)
        {
            Debug.LogError("Unable to create mining selection preview material because no compatible shader was found.");
            return null;
        }

        Material material = new Material(previewShader);
        Color previewColor = new Color(1f, 0.92f, 0.016f, 0.2f);

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", previewColor);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", previewColor);
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        return material;
    }

    private void UpdateSelectionPreview(Vector3Int targetPosition)
    {
        if (_selectionPreviewRenderer == null)
        {
            return;
        }

        Vector3Int minPos = Vector3Int.Min(_position1, targetPosition);
        Vector3Int maxPos = Vector3Int.Max(_position1, targetPosition);
        Vector3 size = (Vector3)(maxPos - minPos + Vector3Int.one) + Vector3.one * SelectionPreviewPadding;

        _selectionPreview.transform.position = ((Vector3)(minPos + maxPos)) * 0.5f;
        _selectionPreview.transform.localScale = size;
        _selectionPreviewRenderer.enabled = true;
    }

    private void HideSelectionPreview()
    {
        if (_selectionPreviewRenderer != null)
        {
            _selectionPreviewRenderer.enabled = false;
        }
    }

    private void ClearSelection()
    {
        _isSelectingArea = false;
        _position1 = default;
        _position2 = default;
        HideSelectionPreview();
    }

    private void OnDestroy()
    {
        if (_selectionPreviewMaterial != null)
        {
            Destroy(_selectionPreviewMaterial);
        }
    }
}
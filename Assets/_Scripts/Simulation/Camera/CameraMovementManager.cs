using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class CameraMovementManager : MonoBehaviour, IUpdatable
{
    public float moveSpeed = 20f;
    public float scrollSpeed = 20f;
    public float rotationSpeed = 400f;
    public float minY = -50f;
    public float maxY = 50f;

    [Header("Terrain Collision")]
    [SerializeField]
    private LayerMask terrainLayerMask = 1; // Default layer

    [SerializeField]
    private float minHeightAboveTerrain = 2f;

    private Camera mainCamera;

    [SerializeField]
    [Tooltip("Spotlight to illuminate the scene, if any. Optional.")]
    private GameObject spotLight;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
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
        HandleLightControl();
        HandleMovement();
        HandleRotation();
        HandleZoom();
        ClampPositionToTerrain();
    }

    private void HandleMovement()
    {
        if (mainCamera == null) return;

        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) &&
           !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            return; // No movement keys pressed
        }

        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) direction += Vector3.back;
        if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) direction += Vector3.right;

        if (direction == Vector3.zero) return;



        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            moveSpeed *= 2; // Double speed when shift is pressed
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            moveSpeed /= 2; // Reset speed when shift is released
        }

        // float horizontalInput = Input.GetAxis("Horizontal");
        // float verticalInput = Input.GetAxis("Vertical");

        Vector3 forward = mainCamera.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = mainCamera.transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDirection = (forward * direction.z + right * direction.x).normalized;
        mainCamera.transform.position += moveDirection * moveSpeed * Time.unscaledDeltaTime;
    }

    private void HandleZoom()
    {
        if (mainCamera == null) return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Vector3 position = mainCamera.transform.position;
            position.y += scrollInput * scrollSpeed * 100f * Time.unscaledDeltaTime; // Adjust zoom speed
            position.y = Mathf.Clamp(position.y, minY, maxY);
            mainCamera.transform.position = position;
        }
    }

    private void HandleRotation()
    {
        if (mainCamera == null) return;

        if (Input.GetMouseButton(1)) // Right mouse button for rotation
        {
            float rotationX = Input.GetAxis("Mouse X") * rotationSpeed * Time.unscaledDeltaTime;
            float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.unscaledDeltaTime;

            mainCamera.transform.Rotate(Vector3.up, rotationX, Space.World);
            mainCamera.transform.Rotate(Vector3.left, rotationY);
        }
    }
    
    private void HandleLightControl()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            spotLight.SetActive(!spotLight.activeSelf); // Toggle light on/off
        }
    }

    private void ClampPositionToTerrain()
    {
        if (mainCamera == null) return;

        Vector3 pos = mainCamera.transform.position;
        float currentMinY = minY;

        // Use the current elevation as the base floor (vertical slice)
        if (World.Instance != null)
        {
            currentMinY = Mathf.Max(minY, World.Instance.currentElevation);
        }

        // Raycast from above the world to find the highest point of the visible terrain
        float rayHeight = 100f;
        if (World.Instance != null)
        {
            rayHeight = World.Instance.maxY + 10f;
        }

        Ray ray = new Ray(new Vector3(pos.x, rayHeight, pos.z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, rayHeight + 100f, terrainLayerMask))
        {
            // Ensure we are above the terrain mesh
            currentMinY = Mathf.Max(currentMinY, hit.point.y);
        }

        if (pos.y < currentMinY + minHeightAboveTerrain)
        {
            pos.y = currentMinY + minHeightAboveTerrain;
            mainCamera.transform.position = pos;
        }
    }

}
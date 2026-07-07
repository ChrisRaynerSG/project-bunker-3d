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

    [Header("Smoothing")]
    [SerializeField]
    [Tooltip("Speed multiplier applied while a Shift key is held.")]
    private float shiftSpeedMultiplier = 2f;

    [SerializeField]
    [Tooltip("Approximate time (in seconds) for movement to ease in/out. Lower = snappier, higher = floatier.")]
    private float movementSmoothTime = 0.12f;

    [SerializeField]
    [Tooltip("How quickly the camera rotation catches up to the mouse. Higher = snappier, lower = smoother.")]
    private float rotationSmoothness = 20f;

    [Header("Terrain Collision")]
    [SerializeField]
    private LayerMask terrainLayerMask = 1; // Default layer

    [SerializeField]
    [Tooltip("Radius of the sphere used for camera collision against solid blocks.")]
    private float collisionRadius = 1f;

    [SerializeField]
    [Tooltip("Maximum distance the camera may be pushed each frame to escape when it ends up inside a solid block.")]
    private float maxEscapeDistance = 20f;

    private Camera mainCamera;

    // The camera position at the end of the previous collision resolution. Each frame
    // the intended new position is validated as a step out of this known-good spot.
    private Vector3 previousPosition;
    private bool hasPreviousPosition = false;

    // Smoothed world-space velocity so movement eases in and out instead of snapping
    // on/off; velocitySmoothDampRef is the internal state used by Vector3.SmoothDamp.
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 velocitySmoothDampRef = Vector3.zero;

    // Smoothed rotation deltas so mouse-look eases rather than jitters frame to frame.
    private float smoothedRotationX;
    private float smoothedRotationY;

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
        else
        {
            previousPosition = mainCamera.transform.position;
            hasPreviousPosition = true;
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
        HandleTerrainCollision();
    }

    private void HandleMovement()
    {
        if (mainCamera == null) return;

        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) direction += Vector3.back;
        if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) direction += Vector3.right;

        // Effective speed is recomputed every frame from the CURRENT shift state.
        // The old approach multiplied/divided the stored moveSpeed on shift key-down/up,
        // which desynchronised (and permanently scaled the speed) whenever a shift press
        // or release happened while no movement key was held - e.g. releasing a move key
        // before releasing shift, so the halving was skipped. Computing it fresh here can
        // never drift.
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed *= shiftSpeedMultiplier;
        }

        Vector3 desiredVelocity = Vector3.zero;
        if (direction != Vector3.zero)
        {
            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = mainCamera.transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 moveDirection = (forward * direction.z + right * direction.x).normalized;
            desiredVelocity = moveDirection * speed;
        }

        // Smoothly accelerate toward / decelerate from the desired velocity so movement
        // eases in and out instead of snapping on and off (the main source of the jank).
        // Time.unscaledDeltaTime keeps camera feel independent of the simulation speed.
        currentVelocity = Vector3.SmoothDamp(
            currentVelocity,
            desiredVelocity,
            ref velocitySmoothDampRef,
            movementSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime);

        if (currentVelocity.sqrMagnitude > 0.000001f)
        {
            mainCamera.transform.position += currentVelocity * Time.unscaledDeltaTime;
        }
    }

    private void HandleZoom()
    {
        if (mainCamera == null) return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Vector3 position = mainCamera.transform.position;
            position.y += scrollInput * scrollSpeed * 50f * Time.unscaledDeltaTime; // Adjust zoom speed
            position.y = Mathf.Clamp(position.y, minY, maxY);
            mainCamera.transform.position = position;
        }
    }

    private void HandleRotation()
    {
        if (mainCamera == null) return;

        float targetRotationX = 0f;
        float targetRotationY = 0f;

        if (Input.GetMouseButton(1)) // Right mouse button for rotation
        {
            targetRotationX = Input.GetAxis("Mouse X") * rotationSpeed * Time.unscaledDeltaTime;
            targetRotationY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.unscaledDeltaTime;
        }

        // Ease the applied rotation toward the raw mouse delta (and back to zero when the
        // button is released) so turning is smooth rather than jittery. The exponential
        // factor is framerate-independent.
        float t = 1f - Mathf.Exp(-rotationSmoothness * Time.unscaledDeltaTime);
        smoothedRotationX = Mathf.Lerp(smoothedRotationX, targetRotationX, t);
        smoothedRotationY = Mathf.Lerp(smoothedRotationY, targetRotationY, t);

        if (Mathf.Abs(smoothedRotationX) > 0.0001f || Mathf.Abs(smoothedRotationY) > 0.0001f)
        {
            mainCamera.transform.Rotate(Vector3.up, smoothedRotationX, Space.World);
            mainCamera.transform.Rotate(Vector3.left, smoothedRotationY);
        }
    }
    
    private void HandleLightControl()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            spotLight.SetActive(!spotLight.activeSelf); // Toggle light on/off
        }
    }

    // Resolves camera movement against solid terrain using a volumetric sphere check.
    // The camera is free to fly anywhere through empty (air) space - including caves
    // and beneath tree canopies - because air blocks contribute no collider geometry.
    // It is stopped only where it would overlap an actual solid (collidable) block.
    private void HandleTerrainCollision()
    {
        if (mainCamera == null) return;

        Vector3 targetPos = mainCamera.transform.position;

        // Keep the camera inside its allowed vertical band regardless of collision.
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        // First frame (or if we never captured a start position): just accept the
        // current spot as our known-good reference and move on.
        if (!hasPreviousPosition)
        {
            previousPosition = targetPos;
            hasPreviousPosition = true;
            mainCamera.transform.position = targetPos;
            return;
        }

        Vector3 resolved = ResolveCollision(previousPosition, targetPos);

        // Safety net: even after resolving the move, the camera can still end up
        // inside a solid block - e.g. after a fast move tunnels through a thin wall,
        // or when the visible slice / terrain changes underneath it and a block
        // suddenly occupies its position. In that case, push it back out to the
        // nearest free space so it can never get stuck inside the terrain.
        resolved = DepenetrateFromTerrain(resolved);

        mainCamera.transform.position = resolved;
        previousPosition = resolved;
    }

    // If the given position is inside a solid, visible block, searches outward for the
    // nearest free spot and returns it; otherwise returns the position unchanged. The
    // common (not-overlapping) case costs a single sphere check, so this is cheap to
    // run every frame. Straight up is preferred (that is where the open viewable space
    // above the current slice lives), then the horizontal axes, then straight down.
    private Vector3 DepenetrateFromTerrain(Vector3 position)
    {
        // Cheap early-out for the overwhelmingly common case: nothing overlaps.
        if (!IsBlocked(position))
        {
            return position;
        }

        // Directions to try, ordered by preference. Up first so the camera surfaces
        // toward open air rather than burrowing sideways through the terrain.
        Vector3[] directions =
        {
            Vector3.up,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back,
            Vector3.down
        };

        float step = Mathf.Max(0.25f, collisionRadius * 0.5f);

        // Expand the search radius outward so the closest exit wins; for a given
        // distance the preferred (earlier) direction wins ties.
        for (float distance = step; distance <= maxEscapeDistance; distance += step)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                Vector3 candidate = position + directions[i] * distance;
                candidate.y = Mathf.Clamp(candidate.y, minY, maxY);

                if (!IsBlocked(candidate))
                {
                    return candidate;
                }
            }
        }

        // Could not find a free spot within range; leave the camera where it is
        // rather than teleport it somewhere unexpected.
        return position;
    }

    // Attempts to move from a known-good position to the desired target. If the target
    // is clear, the full move is allowed. If it is blocked, movement is resolved one
    // axis at a time so the camera slides along a collidable surface instead of
    // sticking to it.
    private Vector3 ResolveCollision(Vector3 from, Vector3 to)
    {
        if (!IsBlocked(to))
        {
            return to;
        }

        Vector3 result = from;

        Vector3 tryX = new Vector3(to.x, result.y, result.z);
        if (!IsBlocked(tryX)) result = tryX;

        Vector3 tryY = new Vector3(result.x, to.y, result.z);
        if (!IsBlocked(tryY)) result = tryY;

        Vector3 tryZ = new Vector3(result.x, result.y, to.z);
        if (!IsBlocked(tryZ)) result = tryZ;

        return result;
    }

    // True when a solid, collidable block overlaps the camera sphere at the given
    // position. Only visible chunks live on the terrain layer and only solid blocks
    // contribute geometry to the chunk collider mesh, so air is never "blocked".
    private bool IsBlocked(Vector3 position)
    {
        return Physics.CheckSphere(position, collisionRadius, terrainLayerMask, QueryTriggerInteraction.Ignore);
    }

}
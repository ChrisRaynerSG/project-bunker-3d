using UnityEngine;

public class CameraMovementManager : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float scrollSpeed = 20f;
    public float minY = -50f;
    public float maxY = 50f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    private void HandleMovement()
    {
        if (mainCamera == null) return;

        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) &&
           !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            return; // No movement keys pressed
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            moveSpeed *= 2; // Double speed when shift is pressed
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            moveSpeed /= 2; // Reset speed when shift is released
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;

        Vector3 moveDirection = (forward * verticalInput + right * horizontalInput).normalized;
        mainCamera.transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void HandleZoom()
    {
        if (mainCamera == null) return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            Vector3 position = mainCamera.transform.position;
            position.y -= scrollInput * scrollSpeed * 100f * Time.deltaTime; // Adjust zoom speed
            position.y = Mathf.Clamp(position.y, minY, maxY);
            mainCamera.transform.position = position;
        }
    }

    private void HandleRotation()
    {
        if (mainCamera == null) return;

        if (Input.GetMouseButton(1)) // Right mouse button for rotation
        {
            float rotationSpeed = 100f;
            float rotationX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            mainCamera.transform.Rotate(Vector3.up, rotationX, Space.World);
            mainCamera.transform.Rotate(Vector3.left, rotationY);
        }
    }

    // @todo: need to set the minY as the current vertical slice so that we don't go underneath the world
}
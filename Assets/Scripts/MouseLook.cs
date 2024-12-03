// MouseLook.cs
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    [Tooltip("Sensitivity of the mouse movement.")]
    public float sensitivity = 3f;

    // Rotation angles
    private float pitch = 0f; // Vertical rotation
    private float yaw = 0f;   // Horizontal rotation

    void Start()
    {
        // Initialize Rotation to face -X direction
        pitch = 0f;
        yaw = -90f; // -90 degrees to face along the -X axis

        // Apply the initial rotation
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Lock the Cursor at Start if No UI is Active
        if (UIManager.Instance != null && !UIManager.Instance.IsAnyUIPanelActive())
        {
            LockCursor();
        }
        else
        {
            UnlockCursor();
        }

        // Subscribe to UIManager Events
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIOpened += OnUIOpened;
            UIManager.Instance.OnUIClosed += OnUIClosed;
        }
        else
        {
            Debug.LogError("UIManager.Instance is null. Please ensure UIManager is added to the scene.");
        }
    }

    void Update()
    {
        // Do Not Process Input if UI is Active
        if (UIManager.Instance != null && UIManager.Instance.IsAnyUIPanelActive())
        {
            return;
        }

        HandleMouseInput();
    }


    // Processes mouse movement input to rotate the camera.
    private void HandleMouseInput()
    {
        // Get mouse movement input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Accumulate rotation angles
        yaw += mouseX;
        pitch -= mouseY; // Subtract to invert Y-axis for natural look

        // Clamp the vertical rotation to prevent flipping
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        // Apply rotation to the camera
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }


    // Locks the cursor and hides it.
    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Unlocks the cursor and makes it visible.
    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    // Event handler for when a UI panel is opened.
    // Unlocks the cursor and disables mouse look.
    private void OnUIOpened()
    {
        UnlockCursor();
    }

    // Event handler for when a UI panel is closed.
    // Locks the cursor and enables mouse look.
    private void OnUIClosed()
    {
        LockCursor();
    }

    void OnDestroy()
    {
        // Unsubscribe from UIManager Events to Prevent Memory Leaks
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIOpened -= OnUIOpened;
            UIManager.Instance.OnUIClosed -= OnUIClosed;
        }
    }
}

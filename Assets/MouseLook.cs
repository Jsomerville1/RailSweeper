using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{


    public float mouseSensitivity = 100f;
    public Transform playerBody;  // Reference to the player's body for horizontal rotation

    float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Adjust the vertical rotation (looking up and down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Prevent over-rotation

        // Apply rotations
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);  // Vertical rotation
        playerBody.Rotate(Vector3.up * mouseX);  // Horizontal rotation
    }
}

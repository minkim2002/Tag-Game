using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The ball's transform
    public float distance = 10f; // Distance from the ball
    public float height = 5f; // Height above the ball
    public float sensitivity = 5f; // Mouse sensitivity

    private float pitch = 0f; // Rotation around the x-axis (up/down)
    private float yaw = 0f; // Rotation around the y-axis (left/right)

    void LateUpdate()
    {
        // Get mouse input for rotation
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;

        // Clamp pitch to prevent flipping
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        // Calculate the new position of the camera
        Vector3 offset = new Vector3(0, height, -distance);
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Set the camera's position and rotation
        transform.position = target.position + rotation * offset;
        transform.LookAt(target); // Make sure the camera is always looking at the ball
    }
}


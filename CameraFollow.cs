using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The ball's transform
    private float thirdPersonDistance = 5f; // Distance from the ball (third-person)
    private float thirdPersonHeight = 2.5f; // Height above the ball (third-person)
    private float sensitivity = 5f; // Mouse sensitivity

    private float pitch = 0f; // Rotation around the x-axis (up/down)
    private float yaw = 0f; // Rotation around the y-axis (left/right)

    private bool isFirstPerson = false; // Boolean to track the camera mode

    void LateUpdate()
    {

        if (target == null) return;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isFirstPerson = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            isFirstPerson = true;
        }

        // Get mouse input for rotation
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;

        // Clamp pitch to prevent flipping
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        if (isFirstPerson)
        {
            // First-person view: position the camera at the ball's position
            Vector3 offset = new Vector3(0, 0, -0.5f) ; 
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            transform.position = target.position + rotation * offset;
        }
        else
        {
            // Third-person view: calculate the new position of the camera
            Vector3 offset = new Vector3(0, thirdPersonHeight, -thirdPersonDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            // Set the camera's position based on the third-person offset
            transform.position = target.position + rotation * offset;
        }

        // Make sure the camera is always looking at the ball
        transform.LookAt(target);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}



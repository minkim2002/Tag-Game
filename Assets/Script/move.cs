using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float speed = 5f; // Adjust the speed of the ball
    public float jumpForce = 7f; // Force applied when jumping
    public LayerMask groundLayers; // To detect if the ball is on the ground
    public Transform groundCheck; // A point on the ball to check if it's grounded
    public float groundCheckRadius = 0.2f; // The radius of the ground check
    public Transform cameraTransform; // Reference to the main camera's transform

    private Rigidbody rb;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Freeze the rotation to prevent rolling
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Get the forward direction relative to the camera
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Remove the y component to keep movement horizontal
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate the movement direction based on camera orientation
        Vector3 movement = (cameraForward * moveVertical + cameraRight * moveHorizontal) * speed * Time.deltaTime;

        // Use MovePosition to move the ball without causing it to roll
        rb.MovePosition(rb.position + movement);

        // Check if the ball is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers);

        // Jump when spacebar is pressed and the ball is grounded
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
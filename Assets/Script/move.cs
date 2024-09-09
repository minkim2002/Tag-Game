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
    public float catchDistance = 1.5f; // Distance to catch the target ball

    private Rigidbody rb;
    private bool isGrounded;
    public Transform targetBall;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevent the ball from rolling

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

        // Allow the player to catch the target ball when pressing 'E'
        if (targetBall != null && Input.GetKeyDown(KeyCode.E) && Vector3.Distance(transform.position, targetBall.position) <= catchDistance)
        {
            CatchTarget();
        }
    }

    void CatchTarget()
    {
        // Ensure targetBall is valid before attempting to destroy it
        if (targetBall != null && Vector3.Distance(transform.position, targetBall.position) <= 1.5f)
        {
            AIBall caughtAIBall = targetBall.GetComponent<AIBall>();
            if (caughtAIBall != null)
            {
                // Notify GameManager that this AI ball has been caught and should be removed
                GameManager.Instance.RemoveBall(caughtAIBall);

                // Assign the next target for the player
                GameManager.Instance.AssignNextTargetForPlayer();
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw a line in the Scene view between the AI ball and its target
        if (targetBall != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetBall.position);
        }
    }
}


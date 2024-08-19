using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBall : MonoBehaviour
{
    public Transform targetBall; // The target ball (could be the player ball) to chase
    public float speed = 5f; // Speed of the AI ball
    public float jumpForce = 7f; // Force applied when jumping
    public LayerMask groundLayers; // To detect if the ball is on the ground
    public Transform groundCheck; // A point on the ball to check if it's grounded
    public float groundCheckRadius = 0.2f; // The radius of the ground check

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevent the AI ball from rolling
    }

    void Update()
    {
        // Calculate the direction towards the target ball
        Vector3 direction = (targetBall.position - transform.position).normalized;
        direction.y = 0; // Keep movement horizontal

        // Move the AI ball towards the target
        Vector3 movement = direction * speed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        // Check if the ball is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers);

        // AI logic to decide when to jump
        if (isGrounded && ShouldJump())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // Example AI logic to decide when to jump
    bool ShouldJump()
    {
        // Simple example: jump if the AI ball is close to the player ball
        float distanceToTarget = Vector3.Distance(transform.position, targetBall.position);
        return distanceToTarget < 3f; // Jump if close enough
    }
}


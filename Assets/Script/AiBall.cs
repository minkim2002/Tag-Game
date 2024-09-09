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
    public float catchDistance = 1.5f; // Distance to catch the target ball

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

        // Check if the AI ball is close enough to catch the target ball
        if (Vector3.Distance(transform.position, targetBall.position) <= catchDistance)
        {
            CatchTarget();
        }
    }

    // Example AI logic to decide when to jump
    bool ShouldJump()
    {
        // Simple example: jump if the AI ball is close to the player ball
        float distanceToTarget = Vector3.Distance(transform.position, targetBall.position);
        return distanceToTarget < 3f; // Jump if close enough
    }

    void CatchTarget()
    {
        // Catch the target
        AIBall caughtAIBall = targetBall.GetComponent<AIBall>();
        if (caughtAIBall != null)
        {
            // Notify GameManager that this AI ball has been caught and should be removed
            GameManager.Instance.RemoveBall(caughtAIBall);

            // Assign the next target for this AI ball
            GameManager.Instance.AssignNextTargetForAI(this);
        }
        else if (targetBall.GetComponent<Move>() != null)
        {
            // If the target is the player ball, trigger game over
            GameManager.Instance.CheckGameOver();
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




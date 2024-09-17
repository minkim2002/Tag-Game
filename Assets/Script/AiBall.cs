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
    public bool isTeamLeader = true;

    private Rigidbody rb;
    private bool isGrounded;
    public List<AIBall> teamMembers;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevent the AI ball from rollingte
        teamMembers = new List<AIBall>();
    }

    void Update()
    {


        
        MoveTowardsTarget();
        
        

        // Check if the ball is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers);

        // AI logic to decide when to jump
        if (isGrounded && ShouldJump())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Check if the AI ball is close enough to catch the target ball
        if (isTeamLeader && Vector3.Distance(transform.position, targetBall.position) <= catchDistance)
        {
            CatchTarget();
        }
    }

    // Method for the team leader to move towards the target
    void MoveTowardsTarget()
    {
        if (targetBall != null)
        {
            Vector3 direction = (targetBall.position - transform.position).normalized;
            direction.y = 0; // Keep movement horizontal

            // Move the AI ball towards the target
            Vector3 movement = direction * speed * Time.deltaTime;
            rb.MovePosition(rb.position + movement);
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
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (targetBall.GetComponent<Move>() != null)
        {
            // If the target is the player ball, trigger game over
            gameManager.CheckGameOver();
        }
        else if (caughtAIBall != null)
        {
            // Add the caught AI ball to the team
            gameManager.HandleTeamFormation(this, caughtAIBall);
            

        }
        
    }

    // Add the caught AI ball to this AI ball's team
    public void AddToTeam(AIBall newTeamMember)
    {
        newTeamMember.isTeamLeader = false; // The caught AI ball is no longer a team leader
        teamMembers.Add(newTeamMember); // Add to the team
        newTeamMember.ChangeColor(this.GetComponent<Renderer>().material.color); // Change color to match the team leader's color
    }

    // Change the color of this AI ball (and its team members)
    public void ChangeColor(Color newColor)
    {
        // Change the color of this ball
        GetComponent<Renderer>().material.color = newColor;

       
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




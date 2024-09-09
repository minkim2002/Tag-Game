using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public AIBall[] aiBalls; // Array of all AI balls in the game
    public Move playerBall; // Reference to the player-controlled ball
    private List<AIBall> activeAIBalls; // List to track active AI balls
    private int currentTargetIndex = 0; // Index to track the current target in the sequence

    void Awake()
    {
        // Singleton pattern to ensure only one instance of GameManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize active AI balls list
        activeAIBalls = new List<AIBall>(aiBalls);
        // Assign initial targets
        AssignInitialTargets();
    }

    void AssignInitialTargets()
    {
        // Loop through all AI balls
        for (int i = 0; i < activeAIBalls.Count; i++)
        {
            if (i < activeAIBalls.Count - 1)
            {
                // Assign the next AI ball as the target for the current AI ball
                activeAIBalls[i].targetBall = activeAIBalls[i + 1].transform;
            }
            else
            {
                // The last AI ball targets the player ball
                activeAIBalls[i].targetBall = playerBall.transform;
            }
        }
        // Player ball's initial target is the first AI ball
        playerBall.targetBall = activeAIBalls[0].transform;
    }

    public void AssignNextTargetForPlayer()
    {
        // Update the player ball's target
        currentTargetIndex = (currentTargetIndex + 1) % activeAIBalls.Count;
        playerBall.targetBall = activeAIBalls[currentTargetIndex].transform;
    }

    public void AssignNextTargetForAI(AIBall aiBall)
    {
        // Update the AI ball's target
        int index = activeAIBalls.IndexOf(aiBall);

        if (index != -1)
        {
            if (index == activeAIBalls.Count - 1)
            {
                // Last AI ball targets the player ball
                aiBall.targetBall = playerBall.transform;
            }
            else
            {
                // Otherwise, assign the next AI ball in the sequence
                aiBall.targetBall = activeAIBalls[index + 1].transform;
            }
        }
    }

    public void RemoveBall(AIBall ball)
    {
        if (activeAIBalls.Contains(ball))
        {
            activeAIBalls.Remove(ball);
            Destroy(ball.gameObject);
            // If player’s target was the removed ball, update to the next
            if (playerBall.targetBall == ball.transform)
            {
                AssignNextTargetForPlayer();
            }

            // Reassign the targets for AI balls that targeted the removed ball
            foreach (AIBall aiBall in activeAIBalls)
            {
                if (aiBall.targetBall == ball.transform)
                {
                    AssignNextTargetForAI(aiBall);
                }
            }
        }
    }

    public void CheckGameOver()
    {
        // Check if the player ball is caught (targeted by the last AI ball)
        if (activeAIBalls.Count > 0 &&
            activeAIBalls[activeAIBalls.Count - 1].targetBall == playerBall.transform &&
            Vector3.Distance(activeAIBalls[activeAIBalls.Count - 1].transform.position, playerBall.transform.position) <= 1.5f)
        {
            // Trigger game over
            Time.timeScale = 0;
            // Implement logic to restart the game or show a lose screen
        }
    }
}






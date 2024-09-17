using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{


    

    public AIBall[] aiBalls; // Array of all AI balls in the game
    public Move playerBall; // Reference to the player-controlled ball
    public List<AIBall> activeAIBalls; // List to track active AI balls

    private int currentTargetIndex = 0; // Index to track the current target in the sequence


    void Start()


    {

        
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

    public void HandleTeamFormation(AIBall leader, AIBall newTeamMember)
    {
        activeAIBalls.Remove(newTeamMember);
        AssignNextTargetForAI(leader);

        foreach (AIBall member in leader.teamMembers)
        {
            member.targetBall = leader.targetBall;
        }
        // Loop through the caught AI ball's team members and add them to the player's team
        foreach (AIBall teamMember in newTeamMember.teamMembers)
        {
            leader.AddToTeam(teamMember);
            teamMember.targetBall = leader.targetBall;
        }

        // Add the caught AI ball to the leader's team and reassign the next target
    
        leader.AddToTeam(newTeamMember);
        newTeamMember.targetBall = leader.targetBall;
        newTeamMember.teamMembers.Clear();
        
    }

    public void HandlePlayerTeamFormation(Move player, AIBall newTeamMember)
    {
        // Add the caught AI ball to the player's team
        activeAIBalls.Remove(newTeamMember);

        // The player continues to chase its original target
        AssignNextTargetForPlayer();

        foreach (AIBall member in player.teamMembers)
        {
            member.targetBall = player.targetBall;
        }
        // Loop through the caught AI ball's team members and add them to the player's team
        foreach (AIBall teamMember in newTeamMember.teamMembers)
        {
            player.AddToTeam(teamMember);
            teamMember.targetBall = player.targetBall;
        }

        player.AddToTeam(newTeamMember);
        newTeamMember.targetBall = player.targetBall;
        newTeamMember.teamMembers.Clear();
        
    }

    public void CheckGameOver()
    {
        // Check if the player ball is caught (targeted by the last AI ball)
        if ((activeAIBalls.Count > 0 &&
            activeAIBalls[activeAIBalls.Count - 1].targetBall == playerBall.transform &&
            Vector3.Distance(activeAIBalls[activeAIBalls.Count - 1].transform.position, playerBall.transform.position) <= 1.5f) || activeAIBalls.Count == 0)
        {
            // Trigger game over
            
            SceneManager.LoadSceneAsync(2);
            
            // Implement logic to restart the game or show a lose screen
        }
    }

   


}






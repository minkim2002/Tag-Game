using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using TMPro;
using Unity.Services.Lobbies.Models;


public class GameManager : NetworkBehaviour
{
    public event EventHandler OnStateChanged;
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private TextMeshProUGUI timerText;

    private bool isFinalMode = false;
    private NetworkVariable<int> finalRoundCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> currentTurnPlayerIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> turnTimer = new NetworkVariable<float>(30f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<float> gameTimer = new NetworkVariable<float>(180f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> countdownTimer = new NetworkVariable<float>(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public Move playerBall;
    public static GameManager Instance { get; private set; }
    public List<Move> activePlayers = new List<Move>();
    private List<Move> allPlayers = new List<Move>();






    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {

        if (!IsServer) return;

        if (state.Value == State.CountdownToStart){
            countdownTimer.Value -= Time.deltaTime;
            if (countdownTimer.Value <= 0f)
            {
                state.Value = State.GamePlaying;
                gameTimer.Value = 180f;
            }
        }
        else if (state.Value == State.GamePlaying)
        {
            if (isFinalMode)
            {
                HandleFinalMode();
            }
            else
            {
                gameTimer.Value -= Time.deltaTime;
                if (gameTimer.Value <= 0f)
                {
                    EndGameDueToTimeout();
                }

                if (activePlayers.Count == 2 && !isFinalMode)
                {
                    EnterFinalMode();
                }
            }
        }
            
    }

    private void EnterFinalMode()
    {
        isFinalMode = true;
        gameTimer.Value = 0f; // Stop regular timer
        turnTimer.Value = 30f; // Set turn timer
        finalRoundCount.Value = 0; // Reset round count
        currentTurnPlayerIndex.Value = 0; // First player starts

        Debug.Log("Final Mode Activated! Alternating turns.");
        activePlayers[0].SetCanCatch(true);
        activePlayers[1].SetCanCatch(false);
       
    }

    private void HandleFinalMode()
    {
        turnTimer.Value -= Time.deltaTime;

        if (turnTimer.Value <= 0f)
        {
            // Switch turn to the next player
            currentTurnPlayerIndex.Value = (currentTurnPlayerIndex.Value + 1) % 2;
            turnTimer.Value = 30f;

            // If we’ve completed 3 rounds, end the game
            if (currentTurnPlayerIndex.Value == 0)
            {
                finalRoundCount.Value++;
                if (finalRoundCount.Value >= 3)
                {
                    Debug.Log("Final Mode: Time's up! No one caught, game over.");
                    EndGameDueToTimeout();
                    return;
                }
            }
            
            StartNextTurn();
        }
    }

    private void StartNextTurn()
    {
        Debug.Log($"Final Mode: Player {activePlayers[currentTurnPlayerIndex.Value].OwnerClientId} is now active!");

        if (activePlayers[0].canCatch.Value)
        {
            activePlayers[0].SetCanCatch(false);
            activePlayers[1].SetCanCatch(true);
        }
        else
        {
            activePlayers[0].SetCanCatch(true);
            activePlayers[1].SetCanCatch(false);
        }

    }


    private void InitializeMultiplayer()
    {
        Debug.Log("Initializaition process started");
        // Assign initial target to each player
        AssignInitialTargetMulti();
    }

    //Multiplayer aspect
    private void AssignInitialTargetMulti()
    {
        activePlayers.RemoveAll(player => player == null);
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (i < activePlayers.Count - 1)
            {
                // Assign the next AI ball as the target for the current AI ball
                activePlayers[i].setTargetBall(activePlayers[i + 1].transform);
            }
            else
            {
                // The last AI ball targets the player ball
                activePlayers[i].setTargetBall(activePlayers[0].transform);
            }
            Debug.Log(activePlayers[i].OwnerClientId + "->" + activePlayers[i].getTargetBall().GetComponent<Move>().OwnerClientId);
        }
    }

    private void ReassignTargets() 
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            // Calculate the index of the next target
            int nextTargetIndex = (i + 1) % activePlayers.Count;

            // Assign the target for the current player
            activePlayers[i].setTargetBall(activePlayers[nextTargetIndex].transform);
            Debug.Log(activePlayers[i].OwnerClientId + "->" + activePlayers[i].getTargetBall().GetComponent<Move>().OwnerClientId);
        }
        CheckGameOver();
    }

    public void HandlePlayerCaught(Move catcher, Move caughtPlayer)
    {
        if (IsServer)
        {
            int place = activePlayers.Count;
            ulong caughtClientId = caughtPlayer.OwnerClientId;

            GameMultiplayer.Instance.SetPlayerPlacement(caughtClientId, place);
        }
        

        Debug.Log($"[GameManager] HandlePlayerCaught called. Catcher: {catcher.OwnerClientId}, CaughtPlayer: {caughtPlayer.OwnerClientId}");
        activePlayers.Remove(caughtPlayer);
        CheckGameOver();
        ReassignTargets();

        foreach (Move member in catcher.getTeamMembersMulti())
        {
            member.setTargetBall(catcher.getTargetBall());
        }
        //Loop through the caught ball's team members and add them to the player's team
        foreach (Move teamMember in caughtPlayer.getTeamMembersMulti())
        {
            catcher.AddToTeamMultiPlayer(teamMember);
            teamMember.setTargetBall(catcher.getTargetBall());
        }

        catcher.AddToTeamMultiPlayer(caughtPlayer);
        Color newColor = catcher.GetComponentInChildren<PlayerVisual>().GetPlayerColor();
        caughtPlayer.GetComponentInChildren<PlayerVisual>().UpdatePlayerColorClientRpc(newColor);
        caughtPlayer.setTargetBall(catcher.getTargetBall());
        caughtPlayer.getTeamMembersMulti().Clear();

        
    }

    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;
        gameTimer.OnValueChanged += UpdateTimerUI;
        countdownTimer.OnValueChanged += UpdateTimerUI;
        turnTimer.OnValueChanged += UpdateTimerUI;
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void UpdateTimerUI(float oldValue, float newValue)
    {
        if (timerText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(newValue);
            timerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {

        Vector3[] spawnPositions = new Vector3[]
        {
            new Vector3(0, 10, -36),
            new Vector3(32, 10, 24),
            new Vector3(-3, 4, 27),
            new Vector3(24, 4, 27),
            new Vector3(36, 10, 8),
            new Vector3(36.5f, 2, -36),
            new Vector3(-36, 10, -36) // Last player in the center
        };
        int spawnIndex = 0;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log($"Spawning player for client {clientId}");
            Transform playerTransform = Instantiate(playerPrefab, spawnPositions[spawnIndex], Quaternion.identity);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            
            Move move = playerTransform.GetComponent<Move>();
          
            if (move != null)
            {
                activePlayers.Add(move);
                allPlayers.Add(move);
                move.SetCanMove(false);
                Debug.Log($"[GameManager] Assigned Move to client {clientId} | Owner: {move.OwnerClientId}");
            }

            spawnIndex = (spawnIndex + 1) % spawnPositions.Length;

        }
        InitializeMultiplayer();

        if (IsServer)
        {
            state.Value = State.CountdownToStart;
            countdownTimer.Value = 5f;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
        if (newValue == State.GamePlaying)
        {
            EnablePlayerMovement();
        }
    }

    private void EnablePlayerMovement()
    {
        foreach (Move player in activePlayers)
        {
            player.SetCanMove(true); // Enable movement when the game starts
        }
    }

    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }

    public void CheckGameOver()
    {
        // Multiplayer-specific game-over check
        if (activePlayers.Count == 1)
        {
            ulong winnerId = activePlayers[0].OwnerClientId;
            GameMultiplayer.Instance.SetPlayerPlacement(winnerId, 1);
            foreach (Move player in allPlayers)
            {
                if (player != null && player.NetworkObject != null && player.NetworkObject.IsSpawned)
                {
                    player.NetworkObject.Despawn();
                }
            }
            activePlayers.Clear();
            allPlayers.Clear();
            Loader.LoadNetwork(Loader.Scene.RestartScene);
        }
    }

    

    private void EndGameDueToTimeout()
    {
        Debug.Log("Game over: Time limit reached.");
        state.Value = State.GameOver;

        if (activePlayers.Count == 2)
        {
            GameMultiplayer.Instance.SetPlayerPlacement(activePlayers[0].OwnerClientId, 1);
            GameMultiplayer.Instance.SetPlayerPlacement(activePlayers[1].OwnerClientId, 1);
        }

        foreach (Move player in allPlayers)
        {
            if (player != null && player.NetworkObject != null && player.NetworkObject.IsSpawned)
            {
                player.NetworkObject.Despawn();
            }
        }
        activePlayers.Clear();
        allPlayers.Clear();

        Loader.LoadNetwork(Loader.Scene.RestartScene);
    }

    


    


}

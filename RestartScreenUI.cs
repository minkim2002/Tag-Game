using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartScreenUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI placementText;

    [SerializeField] private Transform playerPrefab;
    private List<Move> freeRoamPlayers = new List<Move>();

    private void Start()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnRestartSceneLoaded;
    }
    private void OnRestartSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == Loader.Scene.RestartScene.ToString())
        {
            // Safe to spawn players now
            SpawnPlayersForFreeRoam(); // custom method
            StartCoroutine(CountdownToReturnToTitle());

            // Unsubscribe after use
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnRestartSceneLoaded;
        }
    }


    private void SpawnPlayersForFreeRoam()
    {
        Vector3[] orderedSpawnPositions = new Vector3[]
        {
        new Vector3(0, 3.5f, 3),     // 1st place
        new Vector3(0, 2.5f, 1),     // 2nd place
        new Vector3(0, 1.5f, -1),    // 3rd place
        new Vector3(-10, 0.5f, -10),
        new Vector3(10, 0.5f, -10),
        new Vector3(-10, 0.5f, 10),
        new Vector3(10, 0.5f, 10)
        };

        Dictionary<int, ulong> placementToClientId = new Dictionary<int, ulong>();
        List<ulong> otherClients = new List<ulong>();

        // Classify players by placement
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            int placement = GameMultiplayer.Instance.GetPlayerPlacement(clientId);
            if (placement >= 1 && placement <= 3 && !placementToClientId.ContainsKey(placement))
            {
                placementToClientId[placement] = clientId;
            }
            else
            {
                otherClients.Add(clientId);
            }
        }

        int spawnIndex = 0;

        // Spawn 1st, 2nd, 3rd place
        for (int place = 1; place <= 3; place++)
        {
            if (placementToClientId.TryGetValue(place, out ulong clientId))
            {
                Vector3 spawnPos = orderedSpawnPositions[spawnIndex];
                SpawnPlayerAt(clientId, spawnPos);
                spawnIndex++;
            }
        }

        // Spawn the rest
        foreach (ulong clientId in otherClients)
        {
            if (spawnIndex < orderedSpawnPositions.Length)
            {
                SpawnPlayerAt(clientId, orderedSpawnPositions[spawnIndex]);
                spawnIndex++;
            }
            else
            {
                // Fallback if more than 7 players
                Vector3 fallbackPos = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
                SpawnPlayerAt(clientId, fallbackPos);
            }
        }
    }

    private void SpawnPlayerAt(ulong clientId, Vector3 position)
    {
        Transform player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        Move move = player.GetComponent<Move>();
        if (move != null)
        {
            move.EnableFreeRoamMode();
            freeRoamPlayers.Add(move);
        }
    }


    private IEnumerator RequestPlacementAndDisplay()
    {
        

        if (GameMultiplayer.Instance != null)
        {
            Debug.Log("Requesting player placement from the server...");
        
        }

        ulong clientId = NetworkManager.Singleton.LocalClientId;
        Debug.Log("Client ID: " + clientId);
        int placement = GameMultiplayer.Instance.GetPlayerPlacement(clientId);
        Debug.Log($"Fetched Placement: {placement}");

        if (placement > 0)
        {
            placementText.text = $"You placed {placement}{GetPlacementSuffix(placement)}!";
        }
        else
        {
            placementText.text = "Game Over";
        }
        yield return new WaitForSeconds(1f); // Small delay to ensure server is ready
    }

    private IEnumerator CountdownToReturnToTitle()
    {
        yield return new WaitForSeconds(10f); // Wait 10 seconds before cleanup


        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }

        if (GameLobby.Instance != null)
        {
            Destroy(GameLobby.Instance.gameObject);
        }

        


        Loader.Load(Loader.Scene.TitleScene);
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }

    private IEnumerator CleanupAfterDelay()
    {
        yield return new WaitForSeconds(10f); // Wait 10 seconds before cleanup

       
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }

        if (GameLobby.Instance != null)
        {
            Destroy(GameLobby.Instance.gameObject);
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }


        Loader.Load(Loader.Scene.TitleScene);

        
    }


    private string GetPlacementSuffix(int placement)
    {
        if (placement == 1) return "st";
        if (placement == 2) return "nd";
        if (placement == 3) return "rd";
        return "th";
    }

    public void RestartGame()
    {
        Loader.Load(Loader.Scene.TitleScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

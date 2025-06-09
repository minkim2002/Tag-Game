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
        StartCoroutine(SetupFreeRoam());
        StartCoroutine(RequestPlacementAndDisplay());
    }

    private IEnumerator SetupFreeRoam()
    {
        yield return new WaitForSeconds(1f); // let network initialize

        SpawnPlayersForFreeRoam(); // custom method
        StartCoroutine(CountdownToReturnToTitle());
    }

    private void SpawnPlayersForFreeRoam()
    {
        Vector3[] spawnPositions = new Vector3[]
        {
        new Vector3(0, 1, 0),
        new Vector3(5, 1, 0),
        new Vector3(-5, 1, 0),
        new Vector3(0, 1, 5),
        new Vector3(0, 1, -5),
        new Vector3(5, 1, 5),
        new Vector3(-5, 1, -5)
        };

        int spawnIndex = 0;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform player = Instantiate(playerPrefab, spawnPositions[spawnIndex % spawnPositions.Length], Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

            Move move = player.GetComponent<Move>();
            if (move != null)
            {
                move.EnableFreeRoamMode();
                freeRoamPlayers.Add(move);
            }

            spawnIndex++;
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

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }


        Loader.Load(Loader.Scene.TitleScene);
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

using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartScreenUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI placementText;

    private void Start()
    {
        
        StartCoroutine(RequestPlacementAndDisplay());
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
        StartCoroutine(CleanupAfterDelay());
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

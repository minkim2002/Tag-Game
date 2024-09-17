using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartScreenUI : MonoBehaviour
{

    

    public void StartSinglePlayer()
    {
        SceneManager.LoadSceneAsync(1);
    }
    public void StartMultiplayer()
    {
        // For now, this button doesn't do anything
        Debug.Log("Multiplayer mode not yet implemented.");
    }
}



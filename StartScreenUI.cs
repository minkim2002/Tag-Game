using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using Unity.Netcode;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private Button playMultiplayerButton;
    [SerializeField] private Button playSingleplayerButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        

        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }

        if (GameLobby.Instance != null)
        {
            Destroy(GameLobby.Instance.gameObject);
        }

        playMultiplayerButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.StartMultiplayerScene);
        });


        playSingleplayerButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.StartMultiplayerScene);
        });


        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        Time.timeScale = 1f;
    }
}



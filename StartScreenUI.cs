using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using Unity.Netcode;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
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

        startButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.StartMultiplayerScene);
        });


        //playSingleplayerButton.onClick.AddListener(() =>
        //{
        //    Loader.Load(Loader.Scene.StartMultiplayerScene);
        //});


        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        Time.timeScale = 1f;
    }
}



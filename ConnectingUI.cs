using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{

    private void Start()
    {
        GameMultiplayer.Instance.OnTryingToJoinGame += GameMultiplayer_OnTryingToJoinGame;
        GameMultiplayer.Instance.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;
        Hide();
    }

    private void GameMultiplayer_OnTryingToJoinGame(object sender, System.EventArgs e)
    {
        Show();
    }

    private void GameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Hide();
    }


    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnTryingToJoinGame -= GameMultiplayer_OnTryingToJoinGame;
        GameMultiplayer.Instance.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;
    }
}

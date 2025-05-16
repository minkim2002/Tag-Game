using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GameMultiplayer.Instance.ChangePlayerColor(colorId);
        });
    }

    private void Start()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultuplayer_OnPlayerDataNetworkListChanged;
        image.color = GameMultiplayer.Instance.GetPlayerColor(colorId);
        UpdateIsSelected();
    }

    private void GameMultuplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        if (GameMultiplayer.Instance.GetPlayerData().colorId == colorId)
        {
            selectedGameObject.SetActive(true);
        } else
        {
            selectedGameObject.SetActive(false);
        }
    }

    private void OnDestory()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultuplayer_OnPlayerDataNetworkListChanged;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetUIManager : MonoBehaviour
{
    public static TargetUIManager Instance { get; private set; }
    [SerializeField] private Image targetIndicator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }



    public void UpdateTargetColor(Color targetColor)
    {
        Debug.Log("okay almost");
        if (Instance.targetIndicator != null)
        {
            Debug.Log("Color Change Detected");
            Instance.targetIndicator.color = targetColor;
        }
    }
}

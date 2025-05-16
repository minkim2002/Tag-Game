using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Renderer body; // Can be either MeshRenderer or SkinnedMeshRenderer

    private Material material;

    private void Awake()
    {
        if (body is MeshRenderer meshRenderer)
        {
            material = new Material(meshRenderer.material);
            meshRenderer.material = material;
        }
        else if (body is SkinnedMeshRenderer skinnedMeshRenderer)
        {
            material = new Material(skinnedMeshRenderer.material);
            skinnedMeshRenderer.material = material;
        }
    }

    public Color GetPlayerColor()
    {
        return material.color;
    }

    public void SetPlayerColor(Color color)
    {
        Debug.Log($"Setting color to {color} for {gameObject.name}");
        material.color = color;
    }

    [ClientRpc]
    public void UpdatePlayerColorClientRpc(Color color)
    {
        Debug.Log($"[ClientRpc] Updating color to {color} for {gameObject.name}");
        SetPlayerColor(color);
    }
}

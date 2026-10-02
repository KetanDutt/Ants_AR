using UnityEngine;

/// <summary>Keeps floating labels aligned with the current AR camera.</summary>
public class Billboard : MonoBehaviour
{
    private Camera arCamera;

    private void Start()
    {
        if (GameManager.instance != null)
            arCamera = GameManager.instance.arCamera;

        if (arCamera == null)
            arCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (arCamera != null)
            transform.LookAt(transform.position + arCamera.transform.forward, arCamera.transform.up);
    }
}

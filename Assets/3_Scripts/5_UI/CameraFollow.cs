using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }
    private void LateUpdate()
    {
        if (mainCamera == null) return;
        transform.forward = mainCamera.transform.forward;
    }
}

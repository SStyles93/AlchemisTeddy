using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraFollow : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 offset;

    private void Awake()
    {
        mainCamera = Camera.main;
        offset = transform.position - transform.parent.position;
    }
    private void LateUpdate()
    {
        if (mainCamera == null) return;
        transform.position = transform.parent.position + offset;
        transform.forward = mainCamera.transform.forward;
    }
}

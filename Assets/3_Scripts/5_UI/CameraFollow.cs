using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private enum ObjectType
    {
        TwoD,
        ThreeD
    }

    [SerializeField] private ObjectType objectType = ObjectType.TwoD;
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
        switch (objectType)
        {
            case ObjectType.TwoD:
                transform.forward = mainCamera.transform.forward;
                break;
            case ObjectType.ThreeD:
                transform.up = mainCamera.transform.forward;
                break;
        }
    }
}

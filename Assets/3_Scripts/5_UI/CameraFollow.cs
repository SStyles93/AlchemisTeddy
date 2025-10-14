using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private WorldObject worldObject = null;
    private ObjectType objectType = ObjectType.TwoD;
    private Camera mainCamera;
    private Vector3 offset;


    private void Awake()
    {
        mainCamera = Camera.main;
        offset = transform.position - transform.parent.position;
        if (worldObject == null)
            objectType = GetComponentInParent<WorldObject>().GetObjectType();
        else
            objectType = worldObject.GetObjectType();
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

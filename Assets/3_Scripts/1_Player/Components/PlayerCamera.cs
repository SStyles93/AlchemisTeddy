using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCamera : MonoBehaviour
{
    public Transform target = null;

    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 6, -6);
    [SerializeField] private float targetHeightOffset = .5f;

    private Camera m_camera;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private Vector2 minMaxZoomDistance = new Vector2(-3, 5);

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 0.25f;
    [SerializeField] private float maxRotationAngle = 5.0f;
    [HideInInspector] public bool isRotating = false;

    private float currentZoomDistance = 0;
    private float currentRotation = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (m_camera == null)
        {
            m_camera = Camera.main;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) target = transform;

        // Handle camera rotation when the middle mouse button is not held down
        if(!isRotating)
        ResetRotation();

        Vector3 tmpCameraPos = Vector3.Lerp(
            m_camera.transform.position,
            cameraOffset + new Vector3(0,(target.position.y + currentZoomDistance),(target.position.z - currentZoomDistance)),
            Time.deltaTime);

        tmpCameraPos.x = cameraOffset.x + (target.position.x + currentRotation);
        m_camera.transform.position = tmpCameraPos;

        Vector3 cameraTarget = target.position;
        cameraTarget.y += targetHeightOffset;
        m_camera.transform.LookAt(cameraTarget);
    }

    /// <summary>
    /// Handle camera zoom based on mouse scroll wheel input
    /// </summary>
    /// <param name="scrollValue"></param>
    public void HandleZoom(float scrollValue)
    {
        if (m_camera == null) return;

        if (scrollValue != 0f)
        {
            // Adjust the current zoom distance based on scroll input
            currentZoomDistance -= scrollValue * zoomSpeed;
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minMaxZoomDistance.x, minMaxZoomDistance.y);
        }
    }

    public void HandleRotation(float rotationValue)
    {
        if(rotationValue != 0f)
        {
            float rotation = rotationValue * rotationSpeed * Time.deltaTime;
            currentRotation -= rotation;
            currentRotation = Mathf.Clamp(currentRotation, -maxRotationAngle, maxRotationAngle);
        }
    }

    private void ResetRotation()
    {
        if (Mathf.Abs(currentRotation) <= 0.01f)
        {
            currentRotation = 0f;
            return;
        }
        currentRotation = Mathf.Lerp(currentRotation, 0f, rotationSpeed * 5 * Time.deltaTime);
        //Mathf.SmoothStep(currentRotation, 0, Time.deltaTime * rotationSpeed * 20.0f);
    }
}

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCamera : MonoBehaviour
{

    [SerializeField] private Vector3 cameraOffset = new Vector3(0,4,-4);
    [SerializeField] private float targetHeightOffset = .5f;

    private Camera m_camera;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private Vector2 minMaxZoomDistance = new Vector2(-3, 5);

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 0.25f;
    [SerializeField] private float maxRotationAngle = 5.0f;

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
        if (m_camera == null) return;

        // Handle camera zoom based on mouse scroll wheel input
        HandleZoom();

        // Handle camera rotation when the middle mouse button is held down
        HandleRotation();

        Vector3 tmpCameraPos = Vector3.Lerp(m_camera.transform.position, cameraOffset + new Vector3(
                0,
                (transform.position.y + currentZoomDistance),
                (transform.position.z - currentZoomDistance)), Time.deltaTime);
        tmpCameraPos.x = cameraOffset.x + (transform.position.x + currentRotation);
        m_camera.transform.position = tmpCameraPos;

        Vector3 cameraTarget = transform.position;
        cameraTarget.y += targetHeightOffset;
        m_camera.transform.LookAt(cameraTarget);
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            // Adjust the current zoom distance based on scroll input
            currentZoomDistance -= scrollInput * zoomSpeed;
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minMaxZoomDistance.x, minMaxZoomDistance.y);
        }
    }

    private void HandleRotation()
    {
        // Check if the middle mouse button is being held down
        if (Input.GetMouseButton(2))
        {
            // Get mouse movement input
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;

            currentRotation -= mouseX;

            currentRotation = Mathf.Clamp(currentRotation, -maxRotationAngle, maxRotationAngle);
        }
        else
        {
            if(Mathf.Abs(currentRotation) <= 0.01f)
            {
                currentRotation = 0f;
                return;
            } 
            currentRotation = Mathf.Lerp(currentRotation, 0f, Time.deltaTime * rotationSpeed * 20.0f);
                //Mathf.SmoothStep(currentRotation, 0, Time.deltaTime * rotationSpeed * 20.0f);
        }
    }
}

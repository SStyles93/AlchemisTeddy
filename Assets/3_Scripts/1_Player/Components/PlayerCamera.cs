using System.Security.Claims;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlayerCamera : MonoBehaviour
{
    public Transform target = null;
    private PlayerAction playerAction;
    private PlayerController playerController;

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
    [HideInInspector] public Vector2 startRotationPosition = -Vector2.one;


    private float currentZoomDistance = 0;
    private float currentRotation = 0f;
    private Vector2 aim;


    private void OnEnable()
    {
        playerController.OnAim += UpdateAimPosition;

        playerAction.OnMiddleClickPressed += StartRotation;
        playerAction.OnMiddleClickReleased += StopRotation;

        playerAction.OnZoom += HandleZoom;
    }
    private void OnDisable()
    {
        playerController.OnAim -= UpdateAimPosition;

        playerAction.OnMiddleClickPressed -= StartRotation;
        playerAction.OnMiddleClickReleased -= StopRotation;

        playerAction.OnZoom -= HandleZoom;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (m_camera == null)
        {
            m_camera = Camera.main;
        }
        playerAction = GetComponent<PlayerAction>();
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) target = transform;

        if (m_camera == null)
        {
            m_camera = Camera.main;
        }

        // Handle camera rotation when the middle mouse button is not held down
        if (!isRotating)
            ResetRotation();
        else
        {
            float rotationDelta = startRotationPosition.x - aim.x;
            HandleRotation(rotationDelta);
        }

        Vector3 tmpCameraPos = Vector3.Lerp(
            m_camera.transform.position,
            cameraOffset + new Vector3(0, (target.position.y + currentZoomDistance), (target.position.z - currentZoomDistance)),
            Time.deltaTime);

        tmpCameraPos.x = cameraOffset.x + (target.position.x + currentRotation);
        m_camera.transform.position = tmpCameraPos;

        Vector3 cameraTarget = target.position;
        cameraTarget.y += targetHeightOffset;
        m_camera.transform.LookAt(cameraTarget);
    }

    public void ClearTarget()
    {
        target = transform;
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
        if (rotationValue != 0f)
        {
            float rotation = rotationValue * rotationSpeed * Time.deltaTime;
            currentRotation -= rotation;
            currentRotation = Mathf.Clamp(currentRotation, -maxRotationAngle, maxRotationAngle);
        }
    }

    private void UpdateAimPosition(Vector2 aimPosition)
    {
        aim = aimPosition;
    }

    private void StartRotation(Vector2 position)
    {
        if (startRotationPosition == -Vector2.one)
            startRotationPosition = position;
        isRotating = true;
    }

    private void StopRotation()
    {
        isRotating = false;
        startRotationPosition = -Vector2.one;
    }

    public void SetTarget(GameObject gameObject)
    {
        target = gameObject.transform;
    }

    private void ResetRotation()
    {
        if (Mathf.Abs(currentRotation) <= 0.01f)
        {
            currentRotation = 0f;
            return;
        }
        currentRotation = Mathf.Lerp(currentRotation, 0f, rotationSpeed * 10 * Time.deltaTime);
        //Mathf.SmoothStep(currentRotation, 0, Time.deltaTime * rotationSpeed * 20.0f);
    }

}

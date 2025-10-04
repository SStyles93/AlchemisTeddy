using UnityEngine;

public class DynamicOcclusionCutoutSystem : MonoBehaviour
{
    [Header("Wall material reference")]
    [Tooltip("The material contained in Project folders that has to be updated")]
    [SerializeField] private Material m_material;

    [Header("Scene objects reference")]
    [Tooltip("The target's tag\nAt Awake this Tag will be searched\nLeave empty if target is manually assigned")]
    [SerializeField] private string targetTag = "";
    [Tooltip("The Camera from which the occlusion is seen")]
    [SerializeField] private Camera m_camera;
    [Tooltip("The target behind the occluded objects\n(ex: Player behind wall)")]
    [SerializeField] public Transform m_target;

    [Header("Cutout parameters")]
    [Min(0.0f)]
    [Tooltip("The radius of the occlusion mask")]
    [SerializeField] float maskRadius = 4f;
    [Range(0.01f, 1.0f)]
    [Tooltip("Speed at which the transitions are made")]
    [SerializeField] float lerpSpeed = 0.09f;
    [Tooltip("Target height correction\nModify if the mask has to be higher or lower on the target")]
    [SerializeField] float targetHeightCorrection = 0.8f;


    [Header("Raycast Behaviour")]
    [Min(0.01f)]
    [SerializeField] float radius = 0.5f;

    [Header("Debug Settings")]
    [SerializeField] bool enableGizmos = false;

    Vector3 direction;
    Vector3 currentSpherePosition;
    Vector3 targetPosition;
    private float currentMaskRadius = 0.0f;
    private float targetMaskRadius = 0.0f;
    private float currentLerpTime = 0.0f;
    bool isHitting = false;

    private void OnDisable()
    {
        m_material.SetVector("_Target_Position", Vector3.zero);
        m_material.SetFloat("_Radius", 0.0f);
    }

    private void Awake()
    {
        if (targetTag != "" && m_target == null)
            m_target = GameObject.FindGameObjectWithTag(targetTag).transform;
        if (m_camera == null)
            m_camera = Camera.main;
    }

    void Start()
    {
        if (m_target == null)
            Debug.LogWarning($"{this} - Start - Target is not set");
        if (m_camera == null)
            Debug.LogWarning($"{this} - Start - Camera is not set");
        if (m_material == null)
            Debug.LogWarning($"{this} - Start - Material is not set");
        if (m_target == null || m_camera == null || m_material == null) return;
        currentSpherePosition = m_target.position;
        currentMaskRadius = 0.0f;
    }

    void Update()
    {
        if (m_target == null || m_camera == null || m_material == null)
        {
            Debug.LogWarning($"{this} - Update Stopped - Unassigned references");
            this.enabled = false;
            return;
        }

        Vector3 targetPosition = m_target.position + (Vector3.up * targetHeightCorrection);
        // Perform Sphere cast from player to camera
        direction = m_camera.transform.position - m_target.position;

        if (Physics.SphereCast(targetPosition, radius, direction, out RaycastHit hitInfo))
        {
            // Cast hits a wall
            if (!isHitting)
            {
                // Reset LerpTime
                isHitting = true;
                currentLerpTime = 0.0f;
            }
            // Set the targetted position to hitPoint & mask radius to defined size
            this.targetPosition = hitInfo.point;
            targetMaskRadius = maskRadius;
        }
        else
        {
            //No hit
            if (isHitting)
            {
                isHitting = false;
                currentLerpTime = 0.0f;
            }
            // Set the target position to target & maks radius to 0
            this.targetPosition = targetPosition;
            targetMaskRadius = 0.0f;
        }

        // Lerp of Position & Radius
        if (currentLerpTime < 1.0f) currentLerpTime += Time.deltaTime * lerpSpeed;
        currentSpherePosition = Vector3.Lerp(currentSpherePosition, this.targetPosition, currentLerpTime);
        currentMaskRadius = Mathf.Lerp(currentMaskRadius, targetMaskRadius, lerpSpeed);

        // Send Target position & radius to material
        m_material.SetVector("_Target_Position", currentSpherePosition);
        m_material.SetFloat("_Radius", currentMaskRadius);
    }

    private void OnDrawGizmosSelected()
    {

        if (m_target == null || m_camera == null || m_material == null || !enableGizmos) return;

        Vector3 origin = m_target.position + (Vector3.up * targetHeightCorrection);
        Vector3 dir = direction.normalized;
        Vector3 end = m_camera.transform.position;



        // Draw start and end spheres
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(end, radius);

        // Draw capsule "sides"
        Gizmos.DrawLine(origin + Vector3.up * radius, end + Vector3.up * radius);
        Gizmos.DrawLine(origin + Vector3.down * radius, end + Vector3.down * radius);
        Gizmos.DrawLine(origin + Vector3.right * radius, end + Vector3.right * radius);
        Gizmos.DrawLine(origin + Vector3.left * radius, end + Vector3.left * radius);

        // If there’s a hit, mark it
        if (Physics.SphereCast(origin, radius, dir, out RaycastHit hit, 50.0f))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hit.point, radius);
        }

    }
}
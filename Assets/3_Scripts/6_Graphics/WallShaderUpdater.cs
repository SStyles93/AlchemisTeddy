using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class WallShaderUpdater : MonoBehaviour
{
    [SerializeField]
    private Camera m_camera;
    [SerializeField]
    public Transform m_player;
    [SerializeField]
    private Material m_material;

    [Header("Cutout parameters")]
    [SerializeField] float maskRadius = 0.5f;
    [SerializeField] float lerpSpeed = 0.02f;
    [SerializeField] float playerHeightCorrection = 0.8f;


    [Header("Raycast Behaviour")]
    [SerializeField] float radius = 0.5f;
    
    Vector3 direction;
    Vector3 currentSpherePosition;
    Vector3 targetPosition;
    private float currentMaskRadius = 0.0f;
    private float targetMaskRadius = 0.0f;
    private float currentLerpTime = 0.0f;
    bool isHitting = false;



    void Start()
    {
        currentSpherePosition = m_player.position;
        currentMaskRadius = 0.0f;
    }

    void Update()
    {
        Vector3 playerPosition = m_player.position + (Vector3.up * playerHeightCorrection);
        // Perform Sphere cast from player to camera
        direction = m_camera.transform.position - m_player.position;

        if (Physics.SphereCast(playerPosition, radius, direction, out RaycastHit hitInfo))
        {
            // Cast hits a wall
            if (!isHitting)
            {
                // Reset LerpTime
                isHitting = true;
                currentLerpTime = 0.0f;
            }
            // Set the targetted position & mask radius
            targetPosition = hitInfo.point;
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
            targetPosition = playerPosition;
            targetMaskRadius = 0.0f;
        }

        if (currentLerpTime < 1.0f) currentLerpTime += Time.deltaTime * lerpSpeed;
        currentSpherePosition = Vector3.Lerp(currentSpherePosition, targetPosition, currentLerpTime);
        currentMaskRadius = Mathf.Lerp(currentMaskRadius, targetMaskRadius, lerpSpeed);

        // Send Sphere/Player position to material
        m_material.SetVector("_PlayerPosition", currentSpherePosition);
        m_material.SetFloat("_Radius", currentMaskRadius);
    }

    private void OnDrawGizmosSelected()
    {
        {
            if (m_player == null) return;

            Vector3 origin = m_player.position;
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
}



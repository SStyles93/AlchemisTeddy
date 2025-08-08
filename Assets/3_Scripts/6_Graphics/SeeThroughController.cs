using System.Collections.Generic;
using UnityEngine;

public class SeeThroughController : MonoBehaviour
{
    [SerializeField] private GameObject m_player;
    [SerializeField] private LayerMask m_wallLayer; // Assign the layer for walls in the Inspector
    [SerializeField] private int m_seeThroughLayer = 12; // Layer for see-through effect
    [SerializeField] private int m_opaqueLayer = 13; // Original layer for opaque walls
    [SerializeField] private float m_playerAboveWallThreshold = 0.1f; // Small threshold for player above wall check

    private Camera m_camera;
    private List<Collider> m_wallColliders = new List<Collider>();

    private void Awake()
    {
        m_camera = Camera.main;
        if (m_camera == null)
        {
            Debug.LogError("SeeThroughController: Main Camera not found! Please ensure your camera is tagged as 'MainCamera'.");
            enabled = false; // Disable script if no camera
            return;
        }

        if (m_player == null)
        {
            m_player = GameObject.FindGameObjectWithTag("Player");
            if (m_player == null)
            {
                Debug.LogError("SeeThroughController: Player GameObject not found! Please ensure your player is tagged as 'Player'.");
                enabled = false; // Disable script if no player
                return;
            }
        }
    }

    void Start()
    {
        // Collect all colliders on the specified wall layer in the scene
        // Assuming walls are tagged 'Wall' for efficient lookup as discussed in analysis
        GameObject[] wallObjects = GameObject.FindGameObjectsWithTag("Wall");
        if (wallObjects.Length == 0)
        {
            Debug.LogWarning("SeeThroughController: No GameObjects with tag 'Wall' found. Ensure your walls are tagged correctly.");
        }

        foreach (GameObject wallObj in wallObjects)
        {
            if (((1 << wallObj.layer) & m_wallLayer) != 0) // Check if wallObj's layer is in the wallLayer mask
            {
                if (wallObj.TryGetComponent<Collider>(out var collider))
                {
                    m_wallColliders.Add(collider);
                }
            }
        }

        if (m_wallColliders.Count == 0)
        {
            Debug.LogWarning("SeeThroughController: No wall colliders found on the specified Wall Layer with the 'Wall' tag. Ensure walls have colliders, are on the correct layer, and are tagged 'Wall'.");
        }
    }

    void Update()
    {
        float playerZ = m_player.transform.position.z;
        float playerY = m_player.transform.position.y;

        foreach (Collider wallCollider in m_wallColliders)
        {
            // Y-axis based override: If player is above the wall, keep it opaque
            if (playerY > wallCollider.bounds.max.y - m_playerAboveWallThreshold)
            {
                wallCollider.gameObject.layer = m_opaqueLayer; // Keep as Wall
                continue; // Skip to the next wall
            }

            // Z-axis based visibility logic
            // If any part of the wall's Z-bounds is lower (smaller Z-value) than the player's Z-position
            if (wallCollider.bounds.min.z < playerZ || wallCollider.bounds.max.z < playerZ)
            {
                wallCollider.gameObject.layer = m_seeThroughLayer; // See through
            }
            else
            {
                wallCollider.gameObject.layer = m_opaqueLayer; // Wall
            }
        }
    }
}



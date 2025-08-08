using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;

[RequireComponent(typeof(CharacterController), typeof(NavMeshAgent))]
public class PlayerInteraction : MonoBehaviour, ISaveable
{
    [Header("Interaction Settings")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("The distance from which the player can execute an interaction.")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 10f;

    // --- Component & State Variables ---
    private NavMeshAgent navMeshAgent;
    private Coroutine followAndInteractCoroutine;
    public static event Action<WorldItem> OnCollect;

    // --- Debug Variable ---
    private Vector3 hitPosition = Vector3.zero;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
    }

    void Update()
    {
        FaceMovementDirection(); 
    }

    public void HandleLeftClick()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue, 2.0f);

        // --- Step 1: Fire a raycast to identify an interactable target ---
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
        {
            // Clear the ground-click debug position since we are targeting an object.
            hitPosition = Vector3.zero;
            StartInteraction(hit.collider.gameObject);
            return;
        }

        // --- Step 2: If no interactable was hit, check for ground to move ---
        if (Physics.Raycast(ray, out RaycastHit groundHit, 100f, groundLayer))
        {
            StopInteraction();
            Move(groundHit.point);
            // Update the debug variable with the click position.
            hitPosition = groundHit.point;
        }
    }

    private void StartInteraction(GameObject target)
    {
        StopInteraction();
        followAndInteractCoroutine = StartCoroutine(FollowAndInteractRoutine(target));
    }

    private void StopInteraction()
    {
        if (followAndInteractCoroutine != null)
        {
            StopCoroutine(followAndInteractCoroutine);
            followAndInteractCoroutine = null;
        }
    }

    private IEnumerator FollowAndInteractRoutine(GameObject target)
    {
        navMeshAgent.SetDestination(target.transform.position);

        while (Vector3.Distance(transform.position + Vector3.up, target.transform.position) > interactionDistance)
        {
            yield return null;
        }

        navMeshAgent.ResetPath();

        if (target.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(15);
        }
        if (target.TryGetComponent<IActivatable>(out var activatable))
        {
            activatable.Activate(this.gameObject);
        }
        if (target.TryGetComponent<ICollectable>(out var collectable))
        {
            OnCollect?.Invoke(collectable as WorldItem);
            collectable.Collect(transform.GetComponent<PlayerInventoryManager>());
        }

        followAndInteractCoroutine = null;
    }

    public void Move(Vector3 destination)
    {
        navMeshAgent.SetDestination(destination);
    }

    private void FaceMovementDirection()
    {
        if (navMeshAgent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direction = navMeshAgent.velocity.normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    // --- GIZMOS FOR VISUAL DEBUGGING ---

    private void OnDrawGizmos()
    {
        // Don't draw if we haven't clicked anywhere yet.
        if (hitPosition == Vector3.zero) return;

        // Optional: Don't draw if the player has already reached the destination.
        // This check is simple and might not be perfectly accurate if the agent stops slightly short.
        if (Vector3.Distance(transform.position, hitPosition) < 0.5f) return;

        // Draw a green sphere at the last clicked ground position.
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(hitPosition, 0.2f);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a yellow wire sphere representing the interaction distance.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, interactionDistance);
    }


    #region ISaveable Implementation

    /// <summary>
    /// Captures the player's current position and converts it to a string for saving.
    /// </summary>
    public Dictionary<string, string> CaptureState()
    {
        var state = new Dictionary<string, string>();

        // Get the player's current position.
        Vector3 position = transform.position;

        // --- Convert the Vector3 to a string ---
        // We will store it in a format like "x,y,z".
        // Using CultureInfo.InvariantCulture is crucial to ensure '.' is used as the decimal separator.
        string positionString = $"{position.x.ToString(CultureInfo.InvariantCulture)}," +
                                $"{position.y.ToString(CultureInfo.InvariantCulture)}," +
                                $"{position.z.ToString(CultureInfo.InvariantCulture)}";

        // Add the string to our state dictionary with a clear key.
        state.Add("playerPosition", positionString);

        return state;
    }

    /// <summary>
    /// Restores the player's position from the loaded save data.
    /// </summary>
    public void RestoreState(Dictionary<string, string> state)
    {
        // Check if the loaded data contains our position key.
        if (state.TryGetValue("playerPosition", out string positionString))
        {
            // --- Parse the string back into a Vector3 ---

            // 1. Split the string "x,y,z" into an array of three string parts.
            string[] parts = positionString.Split(',');

            // 2. Ensure we have exactly three parts to avoid errors.
            if (parts.Length == 3)
            {
                // 3. Parse each string part back into a float.
                // Using TryParse is safer as it won't throw an error if the string is malformed.
                float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x);
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y);
                float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z);

                // 4. Create the new Vector3 and apply it to the player's transform.
                // Note: For objects with a CharacterController or NavMeshAgent, you may need
                // to use agent.Warp(newPosition) instead of directly setting transform.position
                // to avoid conflicts with the physics/navigation systems.
                transform.position = new Vector3(x, y, z);
            }
            navMeshAgent.destination = transform.position;
        }
    }

    #endregion
}

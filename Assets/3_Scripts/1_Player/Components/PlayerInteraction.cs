using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerInteraction : MonoBehaviour, ISaveable
{
    [Header("Reference Components")]
    [SerializeField] private PlayerAction playerAction;

    [Header("Interaction Settings")]
    [Tooltip("The distance from which the player can execute an interaction.")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 10f;

    private PlayerInventoryManager inventoryManager = null;

    // --- Component & State Variables ---
    private NavMeshAgent navMeshAgent;
    private Coroutine followAndInteractCoroutine;
    public static event Action<WorldItem> OnCollect;

    // --- Private variables ---
    private bool isPointerOverUI = false;
    private bool isInteracting = false;
    private bool enableObjectFacing = true;
    private Moveable currentMoveable = null;
    private Button3D currentButton3D;
    private Coroutine moveAndPlaceCoroutine;

    // --- Debug Variable ---
    private Vector3 hitPosition = Vector3.zero;


    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;

        if (inventoryManager == null) inventoryManager = GetComponent<PlayerInventoryManager>();

        playerAction = GetComponent<PlayerAction>();
    }

    private void OnEnable()
    {
        playerAction.OnLeftClickPressed += HandleLeftClick;
        playerAction.OnLeftClickReleased += HandleLeftClickUp;

        playerAction.OnRightClickPressed += HandleRightClick;
        playerAction.OnRightClickReleased += HandleRightClickUp;

    }

    private void OnDisable()
    {
        playerAction.OnLeftClickPressed -= HandleLeftClick;
        playerAction.OnLeftClickReleased -= HandleLeftClickUp;

        playerAction.OnRightClickPressed -= HandleRightClick;
        playerAction.OnRightClickReleased -= HandleRightClickUp;
    }

    void Update()
    {
        FaceMovementDirection();
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        if (enableObjectFacing && currentMoveable != null) FaceObject(currentMoveable.gameObject);
        if (currentMoveable != null && currentButton3D != null)
        {
            navMeshAgent.SetDestination(currentMoveable.GetPlayerPositionFromButton(currentButton3D));
        }
    }

    public void HandleLeftClick(Ray ray)
    {
        if (inventoryManager.GetInventoryPannel().activeInHierarchy == true && isPointerOverUI) return;

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue, 2.0f);

        //Disable inventory if active and UI is not clicked
        if (inventoryManager.GetInventoryPannel().activeSelf)
            inventoryManager.ToggleInventoryVisibility();

        // Fire a raycast to identify an interactable target
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
        {
            if (isInteracting && currentMoveable != null)
            {
                if (hit.collider.TryGetComponent<Button3D>(out var button))
                {
                    StartPlacement(currentMoveable.GetPlayerPositionFromButton(button));
                    currentButton3D = button;
                    currentButton3D.Press();
                    return;
                }
            }

            // Clear the ground-click debug position since we are targeting an object.
            hitPosition = Vector3.zero;
            StartInteraction(hit.collider.gameObject);

            //Debug.Log($"Raycast - hit position: {hitPosition}");
            return;
        }

        // If no interactable was hit, check for ground to move
        if (Physics.Raycast(ray, out RaycastHit groundHit, 100f, groundLayer))
        {
            if (isInteracting && currentMoveable != null)
            {
                float distance = Vector3.Distance(currentMoveable.transform.position, groundHit.point);
                if (distance < currentMoveable.RaycastBlockingRadius)
                {
                    //Debug.Log("Ground click blocked near interactable!");
                    return;
                }
                else
                {
                    ClearInteractingMode();
                    StopPlacement();
                    //Debug.Log("Ground click cleared interaction mode!");
                }
            }

            StopInteraction();
            Move(groundHit.point);
            // Update the debug variable with the click position.
            hitPosition = groundHit.point;

            //Debug.Log($"Raycast - hit position {groundHit.point}");
        }
    }

    /// <summary>
    /// Handles the Release of the Left click
    /// </summary>
    public void HandleLeftClickUp()
    {
        if (currentButton3D != null)
        {
            currentButton3D.Release();
            currentButton3D = null;
        }
    }

    private void HandleRightClick(Ray ray)
    {
        StopPlacement();
        StopInteraction();
        playerAction.OnLeftClickPressed -= HandleLeftClick;
    }

    private void HandleRightClickUp()
    {
        playerAction.OnLeftClickPressed -= HandleLeftClick;
        playerAction.OnLeftClickPressed += HandleLeftClick;
    }

    public void Move(Vector3 destination)
    {
        navMeshAgent.SetDestination(destination);
    }

    // --- PRIVATE ---
    private void ClearInteractingMode()
    {
        currentMoveable.Disable();
        isInteracting = false;
        currentMoveable = null;
        GetComponent<PlayerCamera>().ClearTarget();
        GetComponent<PlayerAnimatorController>().ClearActiveMoveable();
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

    private void FaceObject(GameObject gameObject)
    {
        if (navMeshAgent.velocity.sqrMagnitude > 0.01f) return;

        Vector3 direction = gameObject.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private IEnumerator FollowAndInteractRoutine(GameObject target)
    {
        navMeshAgent.SetDestination(target.transform.position);

        while (Vector3.Distance(transform.position + Vector3.up, target.transform.position) > interactionDistance)
        {
            yield return null;
        }

        Vector3 lookPos = target.transform.position;
        lookPos.y = 0.5f;
        transform.LookAt(lookPos);

        navMeshAgent.ResetPath();

        if (target.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(10);
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
        if (target.TryGetComponent<Moveable>(out var moveable))
        {
            SetInteractingMode(moveable);
        }

        followAndInteractCoroutine = null;
    }

    private IEnumerator PlacePlayer(Vector3 position)
    {
        GetComponent<PlayerAnimatorController>().ClearActiveMoveable();
        currentMoveable.SetCanMove(false);

        var agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(position);

        while (Vector3.Distance(transform.position, position) > 0.1f)
            yield return null;


        //Debug.Log($"Player placed at {player.transform.position}");

        while (!IsFacing(currentMoveable.gameObject))
        {
            enableObjectFacing = true;
            yield return null;
        }
        enableObjectFacing = false;

        // Activate hand IK Rig
        GetComponent<PlayerAnimatorController>().SetActiveMoveable(currentMoveable);

        currentMoveable.SetCanMove(true);
        moveAndPlaceCoroutine = null;
    }

    private bool IsFacing(GameObject gameObject)
    {
        Vector3 objectDirection = (gameObject.transform.position - transform.position).normalized;
        if (Vector3.Dot(objectDirection, transform.forward) <= 0.98f)
        {
            return false;
        }
        else return true;
    }

    private void SetInteractingMode(Moveable moveable)
    {
        currentMoveable = moveable;
        isInteracting = true;
        GetComponent<PlayerCamera>().SetTarget(currentMoveable.gameObject);
    }

    private void StartInteraction(GameObject target)
    {
        //Debug.Log($"Started interaction with {target}");
        StopInteraction();
        followAndInteractCoroutine = StartCoroutine(FollowAndInteractRoutine(target));
        //Debug.Log($"Coroutine - Started followAndInteractCoroutine");
    }

    public void StartPlacement(Vector3 position)
    {
        StopPlacement();
        moveAndPlaceCoroutine = StartCoroutine(PlacePlayer(position));
    }

    private void StopPlacement()
    {
        if (moveAndPlaceCoroutine != null)
        {
            StopCoroutine(moveAndPlaceCoroutine);
            moveAndPlaceCoroutine = null;
        }
    }

    private void StopInteraction()
    {
        if (followAndInteractCoroutine != null)
        {
            StopCoroutine(followAndInteractCoroutine);
            followAndInteractCoroutine = null;
            //Debug.Log($"Coroutine - Stopped followAndInteractCoroutine");
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
            navMeshAgent.Warp(transform.position);
        }
    }

    #endregion
}

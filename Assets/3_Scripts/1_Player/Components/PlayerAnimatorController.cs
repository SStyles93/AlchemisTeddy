using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class PlayerAnimatorController : MonoBehaviour
{
    // --- Components ---
    [Header("Player Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private PlayerController playerControler;

    // --- Body Parts ---
    [Header("Body Parts")]
    [SerializeField] private Transform rightHand;

    // --- IK ---
    [Header("IK")]
    [SerializeField] private Rig HandIkRig;
    [SerializeField] private Transform rightHandTarget;
    private Vector3 initialRightHandPosition;
    [SerializeField] private Transform leftHandTarget;
    private Vector3 initialLeftHandPosition;


    // --- Animation Parameters ---
    private WorldItem currentlyHeldItem;
    private Moveable currentMoveable = null;

    int MovementSpeed = 0;
    int PickUpTrigger = 0;

    float savedNavMeshSpeed = 0;


    private void OnEnable()
    {
        PlayerInteraction.OnCollect += StartCollectAnimation;
    }

    private void OnDisable()
    {
        PlayerInteraction.OnCollect -= StartCollectAnimation;
    }

    void Awake()
    {
        if (navMeshAgent == null && TryGetComponent<NavMeshAgent>(out var agent))
        {
            navMeshAgent = agent;
        }
        if (animator == null && TryGetComponent<Animator>(out var anim))
        {
            animator = anim;
        }
        if (playerControler == null && TryGetComponent<PlayerController>(out var action))
        {
            playerControler = action;
        }
        if (HandIkRig == null)
        {
            HandIkRig.GetComponentInChildren<Rig>();
        }

        initialRightHandPosition = rightHandTarget.transform.localPosition;
        initialLeftHandPosition = leftHandTarget.transform.localPosition;
    }

    void Start()
    {
        HandIkRig.weight = 0;
        MovementSpeed = Animator.StringToHash("MovementSpeed");
        PickUpTrigger = Animator.StringToHash("PickUp");
    }

    void Update()
    {
        if (animator != null && navMeshAgent != null)
        {
            animator.SetFloat(MovementSpeed, navMeshAgent.velocity.magnitude);
        }
    }

    /// <summary>
    /// Triggers the beginning of the Pickup animation
    /// </summary>
    void StartCollectAnimation(WorldItem item)
    {
        //Sets the reference to the item being collected
        currentlyHeldItem = item;

        if (animator != null) animator.SetTrigger(PickUpTrigger);
        if (navMeshAgent != null)
        {
            savedNavMeshSpeed = navMeshAgent.speed;
            navMeshAgent.speed = 0;
        }
    }

    public void SetActiveMoveable(Moveable movable)
    {
        currentMoveable = movable;
        if (currentMoveable != null)
        {
            PositionHandIK();
            HandIkRig.weight = 1.0f;
        }
        else
        {
            HandIkRig.weight = 0.0f;
            rightHandTarget.transform.localPosition = initialRightHandPosition;
            leftHandTarget.transform.localPosition = initialLeftHandPosition;
        }
    }

    private void PositionHandIK()
    {
        Vector3 rightHandDirection = (currentMoveable.transform.position + currentMoveable.HandPosition) - rightHandTarget.transform.position;
        Ray rightHandRay = new Ray(rightHandTarget.transform.position, rightHandDirection);
        //Debug.DrawRay(rightHandRay.origin, rightHandRay.direction * 10, Color.green, 2.0f);

        //Check if layer is interactable ((1 << 10) = 1024 = interactableLayer)
        if (Physics.Raycast(rightHandRay, out RaycastHit rightHit, 5.0f, 1 << 10))
        {
            rightHandTarget.transform.position = rightHit.point;
        }


        Vector3 leftHandDirection = (currentMoveable.transform.position + currentMoveable.HandPosition) - leftHandTarget.transform.position;
        Ray leftHandRay = new Ray(leftHandTarget.transform.position, leftHandDirection);
        //Debug.DrawRay(leftHandRay.origin, leftHandRay.direction * 10, Color.green, 2.0f);

        if (Physics.Raycast(leftHandRay, out RaycastHit leftHit, 5.0f, 1 << 10))
        {
            leftHandTarget.transform.position = leftHit.point;
        }
    }

    /// <summary>
    /// Method called by the animator to place the Collectable in the player's hand
    /// </summary>
    public void PlaceObjectInHand()
    {
        currentlyHeldItem.GetComponent<Collider>().enabled = false;
        currentlyHeldItem.GetComponent<Rigidbody>().isKinematic = true;
        currentlyHeldItem.transform.position = rightHand.transform.position;
        currentlyHeldItem.transform.parent = rightHand.transform;
        currentlyHeldItem.transform.rotation = rightHand.rotation;
    }

    /// <summary>
    /// Method called by the animator to destroy the collectable at the correct moment
    /// </summary>
    public void EndCollection()
    {
        if (navMeshAgent != null) navMeshAgent.speed = savedNavMeshSpeed;
        Destroy(currentlyHeldItem.gameObject);
        currentlyHeldItem = null;
    }
}

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
        if(currentMoveable != null)
        {
            PositionHandIK();
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
        if(currentMoveable != null) HandIkRig.weight = 1.0f;
        else HandIkRig.weight = 0.0f;
    }
    private void PositionHandIK()
    {
        if (currentMoveable == null) return;

        rightHandTarget.transform.position = currentMoveable.transform.position + currentMoveable.HandPosition;
        leftHandTarget.transform.position = currentMoveable.transform.position + currentMoveable.HandPosition;
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

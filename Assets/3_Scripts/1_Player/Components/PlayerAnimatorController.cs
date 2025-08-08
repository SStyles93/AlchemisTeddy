using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimatorController : MonoBehaviour
{
    // --- Components ---
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private NavMeshAgent navMeshAgent;
    [SerializeField]
    private PlayerActions actions;

    // --- Body Parts ---
    [SerializeField]
    private Transform playerRightHand;

    private WorldItem currentlyHeldItem;

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
        if (actions == null && TryGetComponent<PlayerActions>(out var action))
        {
            actions = action;
        }
    }

    void Start()
    {
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

    /// <summary>
    /// Method called by the animator to playe the Collectable in the player's hand
    /// </summary>
    public void PlaceObjectInHand()
    {
        currentlyHeldItem.GetComponent<Collider>().enabled = false;
        currentlyHeldItem.GetComponent<Rigidbody>().isKinematic = true;
        currentlyHeldItem.transform.position = playerRightHand.transform.position;
        currentlyHeldItem.transform.parent = playerRightHand.transform;
        currentlyHeldItem.transform.rotation = playerRightHand.rotation;
    }
  
    /// <summary>
    /// Method called by the animator to destroy the collectable at the correct moment
    /// </summary>
    public void EndCollection()
    {
        if(navMeshAgent != null) navMeshAgent.speed = savedNavMeshSpeed;
        Destroy(currentlyHeldItem.gameObject);
        currentlyHeldItem = null;
    }
}

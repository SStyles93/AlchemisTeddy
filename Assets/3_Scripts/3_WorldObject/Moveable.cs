using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;


public class Moveable : WorldObject, IActivatable, IMoveable
{
    [Header("Moveable Attributes")]
    [SerializeField] protected GameObject player;
    [SerializeField] protected float playerOffset = .5f;
    [SerializeField] private Vector3 handPosition;

    [SerializeField] private float raycastBlockingRadius = 2f;


    protected List<Button3D> buttons = new List<Button3D>();
    protected bool isPlayerPlaced = false;
    private bool isPlacing = false;
    private Vector3 playerIntendedPosition = Vector3.zero;

    public float RaycastBlockingRadius => raycastBlockingRadius;
    public Vector3 HandPosition => handPosition;


    protected void Awake()
    {
        foreach (Button3D button in GetComponentsInChildren<Button3D>())
        {
            buttons.Add(button);
        }
    }

    private void OnEnable()
    {
        foreach (Button3D button in buttons)
        {
            button.OnPressedEvents += Move;
            button.OnReleasedEvents += ResetPlayerPlacing;
        }
    }

    private void OnDisable()
    {
        foreach (Button3D button in buttons)
        {
            button.OnPressedEvents -= Move;
            button.OnReleasedEvents -= ResetPlayerPlacing;
        }
    }

    public void Activate(GameObject activator)
    {
        player = activator;
        player.GetComponent<PlayerInteraction>().SetInteractingMode(this);
        player.GetComponent<PlayerCamera>().target = this.transform;
        foreach (Button3D button in buttons)
        {
            button.gameObject.SetActive(true);
            button.EnableButton();
        }
    }

    /// <summary>
    /// Gives the opposite position of the Moveable Object
    /// </summary>
    /// <param name="position">The original position</param>
    public Vector3 ComputeOppositePosition(Vector3 position)
    {
        // Compute opposite side of the barrel relative to arrow
        Vector3 direction = (position - transform.position).normalized;
        float offset = playerOffset + 0.5f; // tweak this distance
        Vector3 correctedPosition = transform.position - direction * offset;
        correctedPosition.y = player.transform.position.y;
        return correctedPosition;
    }

    public void Disable()
    {
        player.GetComponent<PlayerCamera>().target = null;
        player.GetComponent<PlayerAnimatorController>().SetActiveMoveable(null);
        foreach (Button3D button in buttons)
        {
            button.DisableButton();
        }
    }

    private void FaceObject()
    {
        Vector3 objectDirection = (transform.position - player.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(objectDirection);
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, lookRotation, Time.deltaTime * 2.0f);
    }

    public virtual void Move(Vector3 position)
    {
        if (!isPlayerPlaced)
        {
            StartPlacement(position);
            return;
        }
        if (!PlayerIsFacing()) return;
    }

    public void ResetPlayerPlacing()
    {

        isPlayerPlaced = false;
        isPlacing = false;
    }

    public void StartPlacement(Vector3 position)
    {
        if (isPlacing) return; // don't restart if already placing
        //Debug.Log($"StartPlacement at {position}");
        isPlacing = true;
        StartCoroutine(PlacePlayer(position));
    }

    private IEnumerator PlacePlayer(Vector3 position)
    {
        isPlayerPlaced = false;
        player.GetComponent<PlayerAnimatorController>().SetActiveMoveable(null);

        Vector3 correctedPosition = ComputeOppositePosition(position);

#if UNITY_EDITOR
        // Gizmo info
        playerIntendedPosition = correctedPosition;
#endif
        var agent = player.GetComponent<NavMeshAgent>();
        agent.SetDestination(correctedPosition);

        while (Vector3.Distance(player.transform.position, correctedPosition) > 0.2f)
            yield return null;

        //Debug.Log($"Player placed at {player.transform.position}");

        // TODO:  //FaceObject(); <== Has to be in and Update !
        while (!PlayerIsFacing())
            yield return null;

        // Activate hand IK Rig
        player.GetComponent<PlayerAnimatorController>().SetActiveMoveable(this);

        isPlayerPlaced = true;
        isPlacing = false;
    }

    public bool PlayerIsFacing()
    {
        Vector3 objectDirection = (transform.position - player.transform.position).normalized;
        if (Vector3.Dot(objectDirection, player.transform.forward) >= 0.9f)
        {
            return true;
        }
        else return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, raycastBlockingRadius);

        Gizmos.color = Color.brown;
        Gizmos.DrawSphere(this.transform.position, playerOffset);

        Gizmos.color = Color.green;
        Gizmos.DrawCube(handPosition, new Vector3(1f, .1f, 1f));

        if (playerIntendedPosition == Vector3.zero) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(playerIntendedPosition, .25f);
    }
}

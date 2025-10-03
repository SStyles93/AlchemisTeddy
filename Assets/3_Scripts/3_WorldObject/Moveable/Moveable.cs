using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;


public class Moveable : WorldObject, IActivatable, IMoveable
{
    protected enum Direction { North, South, East, West};

    [Header("Moveable Attributes")]
    [SerializeField] protected GameObject player;
    [SerializeField] protected float playerOffset = .5f;
    public Vector3 HandPosition => handPosition;
    [SerializeField] private Vector3 handPosition;
    public float RaycastBlockingRadius => raycastBlockingRadius;
    [SerializeField] private float raycastBlockingRadius = 2f;

    [Header("Button Attributes")]
    [SerializeField] protected GameObject buttonPrefab;
    [SerializeField] protected List<Direction> directions = new List<Direction>();
    protected Dictionary<Direction, GameObject> buttons = new Dictionary<Direction, GameObject>();

    protected bool canMove = false;

    // --- Debug ---
    private Vector3 playerIntendedPosition = Vector3.zero;

    protected void Awake()
    {
        float buttonOffset = 1.25f;
        foreach(Direction direction in directions)
        {
            switch (direction)
            {
                case Direction.North:
                    buttons[Direction.North] = Instantiate(buttonPrefab, 
                        transform.position + new Vector3(0, buttonOffset, buttonOffset), 
                        Quaternion.identity, transform);
                    buttons[Direction.North].gameObject.name = "ButtonNorth";
                    break;
                case Direction.South:
                    buttons[Direction.South] = Instantiate(buttonPrefab,
                        transform.position + new Vector3(0, buttonOffset, -buttonOffset), 
                        Quaternion.Euler(new Vector3(0, 180, 0)), 
                        transform);
                    buttons[Direction.South].gameObject.name = "ButtonSouth";
                    break;
                case Direction.East:
                    buttons[Direction.East] = Instantiate(buttonPrefab,
                        transform.position + new Vector3(buttonOffset, buttonOffset, 0),
                        Quaternion.Euler(new Vector3(0, 90, 0)),
                        transform);
                    buttons[Direction.East].gameObject.name = "ButtonEast";
                    break;
                case Direction.West:
                    buttons[Direction.West] = Instantiate(buttonPrefab,
                        transform.position + new Vector3(-buttonOffset, buttonOffset, 0),
                        Quaternion.Euler(new Vector3(0, 270, 0)),
                        transform);
                    buttons[Direction.West].gameObject.name = "ButtonWest";
                    break;
            }
        }
    }

    private void OnEnable()
    {
        foreach (var kvp in buttons)
        {
            buttons[kvp.Key].GetComponent<Button3D>().OnPressedEvents += Move;
        }
    }

    private void OnDisable()
    {
        foreach (var kvp in buttons)
        {
            buttons[kvp.Key].GetComponent<Button3D>().OnPressedEvents -= Move;
        }
    }

    public virtual void Activate(GameObject activator)
    {
        player = activator;
        foreach (var kvp in buttons)
        {
            buttons[kvp.Key].GetComponent<Button3D>().EnableButton();
        }
    }

    /// <summary>
    /// Gives the opposite position of the button Object
    /// </summary>
    /// <param name="position">The original position</param>
    public Vector3 GetPlayerPositionFromButton(Button3D button)
    {
        // Compute opposite side of the barrel relative to arrow
        Vector3 direction = (button.transform.position - transform.position).normalized;
        float offset = playerOffset + 0.5f; // tweak this distance
        Vector3 correctedPosition = transform.position - direction * offset;
        correctedPosition.y = player.transform.position.y;
        playerIntendedPosition = correctedPosition;
        return correctedPosition;
    }

    public void Disable()
    {
        SetCanMove(false);
        foreach (var kvp in buttons)
        {
            buttons[kvp.Key].GetComponent<Button3D>().DisableButton();
        }
        player = null;
    }

    public virtual void Move(Vector3 position)
    {
       
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    private void OnDrawGizmosSelected()
    {
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(this.transform.position, raycastBlockingRadius);

    //    Gizmos.color = Color.green;
    //    Gizmos.DrawCube(handPosition, new Vector3(1f, .1f, 1f));

        Gizmos.color = Color.brown;
        Gizmos.DrawSphere(this.transform.position, playerOffset);

        if (playerIntendedPosition == Vector3.zero) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(playerIntendedPosition, .25f);
    }
}

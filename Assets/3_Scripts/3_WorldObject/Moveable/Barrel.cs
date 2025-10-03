using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

public class Barrel : Moveable
{
    [Header("Barrel Attributes")]
    [SerializeField] private float movementSpeed = 1.0f;
    [SerializeField] private float checkDistance = 1.1f; // slightly bigger than half of your barrel size
    [SerializeField] private LayerMask obstacleMask; // layer for walls/objects


    private new void Awake()
    {
        base.Awake();
    }
    private void Update()
    {
        if (player == null) return;
        CheckDirections();
    }

    public override void Activate(GameObject activator)
    {
        player = activator;
    }
    private void CheckDirections()
    {
        foreach (var kvp in buttons)
        {
            switch (kvp.Key)
            {
                case Direction.North:
                    // North (+Z)
                    CheckDirection(Vector3.forward, buttons[kvp.Key]);
                    break;

                case Direction.South:
                    // South (-Z)
                    CheckDirection(Vector3.back, buttons[kvp.Key]);

                    break;

                case Direction.East:
                    // East (+X)
                    CheckDirection(Vector3.right, buttons[kvp.Key]);

                    break;

                case Direction.West:
                    // West (-X)
                    CheckDirection(Vector3.left, buttons[kvp.Key]);
                    break;
            }
        }
    }

    private void CheckDirection(Vector3 direction, GameObject button)
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, checkDistance, obstacleMask))
        {
            button.GetComponent<Button3D>().DisableButton();
            Debug.DrawLine(transform.position + Vector3.up * 0.5f, direction * checkDistance + Vector3.up * 0.5f, Color.red);
        }
        else
        {
            button.GetComponent<Button3D>().EnableButton();
            Debug.DrawLine(transform.position + Vector3.up * 0.5f, direction * checkDistance + Vector3.up * 0.5f, Color.green);
        }
    }

    public override void Move(Vector3 direction)
    {
        if (!canMove) return;
        direction.y = 0.0f;
        // Move using transform
        transform.position += movementSpeed * Time.deltaTime * direction.normalized;
        //Debug.Log($"Moving {this.gameObject.name} to {direction}");
    }
}

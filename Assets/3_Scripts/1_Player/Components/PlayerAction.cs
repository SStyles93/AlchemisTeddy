using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerControler))]
public class PlayerAction : MonoBehaviour
{
    //Reference Scripts
    [SerializeField] private PlayerControler playerController;
    [SerializeField] private PlayerInteraction playerInteraction;

    //Reference GameObjects
    [Header("Player's body parts")]
    [SerializeField] private GameObject target;

    //List of bools used for Actions
    [Header("Action's variables")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _aimCorrection;

    Vector3 currentAimPos;


    void Awake()
    {
        playerInteraction = GetComponent<PlayerInteraction>();
        playerController = GetComponent<PlayerControler>();
    }
    private void Start()
    {
        //aim.transform.localPosition = new Vector3(0.0f, -0.5f, 0.0f);
    }

    /// <summary>
    /// Updates the player look direction
    /// </summary>
    public void AimCheck(Vector2 look)
    {
        //TODO: GET RAY FROM INTERACTIONS
        //TODO: Change colour of target if interactable element

        switch (playerController.ControlScheme)
        {
            case "Gamepad":

                //Updates the Aim position according to the Gamepad input

                if (look != Vector2.zero)
                {
                    target.transform.localPosition = new Vector3(look.x, look.y, 0.0f);
                    target.GetComponent<SpriteRenderer>().enabled = true;
                }
                else
                {
                    target.transform.localPosition = Vector3.zero;
                    target.GetComponent<SpriteRenderer>().enabled = false;
                }
                break;
            case "Keyboard&Mouse":

                //Switches from GamepadTarget to MouseTarget
                target.gameObject.SetActive(true);

                //Updates the Aim position according to the Mouse position 
                Vector3 mousePos = look;
                target.transform.position = mousePos;
                currentAimPos = look;
                break;

            default:
                //mouseTarget.gameObject.SetActive(false);
                break;
        }

    }

    /// <summary>
    /// Handles the Left Click with corrected Aim
    /// </summary>
    public void HandleLeftClick()
    {
        playerInteraction.HandleLeftClick(currentAimPos);
    }
}

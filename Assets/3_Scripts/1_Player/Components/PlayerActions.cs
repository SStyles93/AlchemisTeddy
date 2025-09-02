using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerControler))]
public class PlayerActions : MonoBehaviour
{
    //Reference Scripts
    private PlayerControler playerControler;

    //Reference GameObjects
    [Header("Player's body parts")]
    [SerializeField] private GameObject mouseTarget;
    [SerializeField] private GameObject gamepadTarget;
    [SerializeField] private GameObject aim;

    //List of bools used for Actions
    [Header("Action's variables")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _aimCorrection;

    Vector3 currentAimPos;

    //Properties
    public GameObject Aim { get => aim; private set => aim = value; }

    void Awake()
    {
        playerControler = GetComponent<PlayerControler>();
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
        switch (playerControler.ControlScheme)
        {
            case "Gamepad":

                //Switches from MouseTarget to GamepadTarget
                mouseTarget?.gameObject.SetActive(false);
                gamepadTarget.GetComponent<SpriteRenderer>().enabled = true;

                //Updates the Aim position according to the Gamepad input

                //if (look != Vector2.zero)
                //{
                //    gamepadTarget.transform.localPosition = new Vector3(look.x, look.y, 0.0f);
                //    currentAimPos = aim.transform.localPosition = new Vector3(look.x, look.y, 0.0f);
                //    gamepadTarget.GetComponent<SpriteRenderer>().enabled = true;
                //}
                //else
                //{
                //    gamepadTarget.transform.localPosition = Vector3.zero;
                //    gamepadTarget.GetComponent<SpriteRenderer>().enabled = false;
                //    aim.transform.localPosition = currentAimPos;
                //}
                break;
            case "Keyboard":

                ////Switches from GamepadTarget to MouseTarget
                //mouseTarget.gameObject.SetActive(true);
                //gamepadTarget.GetComponent<SpriteRenderer>().enabled = false;

                ////Updates the Aim position according to the Mouse position 
                //Vector3 mousePos = Input.mousePosition;
                //mousePos.z = 0.0f;
                //mouseTarget.transform.position = mousePos;
                //_aim.transform.localPosition =
                //gamepadTarget.transform.localPosition =
                //    mouseTarget.transform.localPosition.normalized;
                break;

            default:
                //mouseTarget.gameObject.SetActive(false);
                break;
        }

        //Vector3 correctedPos = aim.transform.localPosition;
        //correctedPos.x += correctedPos.x * -_aimCorrection;
        //correctedPos.y += correctedPos.y * -_aimCorrection;
        //aim.transform.localPosition = correctedPos;
    }
}

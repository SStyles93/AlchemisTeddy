using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
public class PlayerAction : MonoBehaviour
{
    //Reference Scripts
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Camera playerCamera;

    // Action params
    [Header("Layer Settings")]
    [SerializeField] private GraphicRaycaster playerScreenSpaceCanvasRaycaster;
    [SerializeField] private LayerMask interactableLayers;

    //Reference GameObjects
    [Header("Player's targer")]
    [SerializeField] private GameObject target;
    private Image pointerImage;

    //List of bools used for Actions
    [Header("Action's variables")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _aimCorrection;

    // --- Private variables ---
    private Ray currentRay;
    private bool isPointerOverUI = false;


    void Awake()
    {
        playerInteraction = GetComponent<PlayerInteraction>();
        if (target != null) pointerImage = target.GetComponent<Image>();
    }
    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        //aim.transform.localPosition = new Vector3(0.0f, -0.5f, 0.0f);
    }

    void Update()
    {
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Updates the player look direction
    /// </summary>
    public void AimCheck(Vector2 mousePosition)
    {
        if (playerCamera == null) playerCamera = Camera.main;


        // Get Ray from mouse position
        currentRay = playerCamera.ScreenPointToRay(mousePosition);

        // Moves the mouse pointer according to the mousePosition
        target.transform.position = mousePosition;

        if (DetectInteractableUIUnderPointer(mousePosition))
        {
            pointerImage.color = Color.green;
            return;
        }
        else if (!isPointerOverUI)
        {
            // If not check with world objects
            if (Physics.Raycast(currentRay, out RaycastHit hit, 100f, interactableLayers))
            {
                pointerImage.color = Color.green;
                //TODO: change image (interaction pointer)
            }
            else
            {
                pointerImage.color = Color.red;
                //TODO: Change image (normal pointer)
            }
        }
        #region ControlScheme TODO
        //switch (playerController.ControlScheme)
        //{
        //    case "Gamepad":

        //        Updates the Aim position according to the Gamepad input

        //        if (mousePosition != Vector2.zero)
        //        {
        //            target.transform.localPosition = new Vector3(mousePosition.x, mousePosition.y, 0.0f);
        //            target.GetComponent<SpriteRenderer>().enabled = true;
        //             Get Ray from mouse position
        //            currentRay = playerCamera.ScreenPointToRay(mousePosition);
        //        }
        //        else
        //        {
        //            target.transform.localPosition = Vector3.zero;
        //            target.GetComponent<SpriteRenderer>().enabled = false;
        //        }
        //        break;
        //    case "Keyboard&Mouse":
        //        Switches from GamepadTarget to MouseTarget
        //        target.gameObject.SetActive(true);

        //        Updates the Aim position according to the Mouse position 
        //        target.transform.position = mousePosition;

        //         Get Ray from mouse position
        //        currentRay = playerCamera.ScreenPointToRay(mousePosition);
        //        break;
        //    default:
        //        currentRay = playerCamera.ScreenPointToRay(mousePosition);
        //        break;
        //}
        #endregion
    }

    /// <summary>
    /// Handles the Left Click with corrected Aim
    /// </summary>
    public void HandleLeftClick()
    {
        playerInteraction.HandleLeftClick(currentRay);
    }

    bool DetectInteractableUIUnderPointer(Vector2 mousePosition)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        playerScreenSpaceCanvasRaycaster.Raycast(pointerData, results);

        if (results.Count > 0)
        {
            // Topmost UI element (first one hit)
            GameObject hitUI = results[0].gameObject;
            if ((interactableLayers & (1 << hitUI.layer)) != 0)
            {
                return true;
            }
            //Debug.Log($"UI Hit: {hitUI.name} | Layer: {LayerMask.LayerToName(hitUI.layer)}");
        }
        //else
        //{
        //    Debug.Log("No UI element under mouse");
        //}

        return false;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerController))]
public class PlayerAction : MonoBehaviour
{
    //Reference Scripts
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Camera mainCamera;

    // Action params
    [SerializeField] private float movementRayDelay = 0.25f;

    ////List of bools used for Actions
    //[Header("Action's variables")]
    //[Range(0.0f, 1.0f)]
    //[SerializeField] private float _aimCorrection;

    // --- Private variables ---
    private Ray currentRay;
    private float currentMovementRayDelay = 0.25f;
    private bool isLeftClickPressed = false;
    private bool isRightClickPressed = false;

    public event Action<Ray> OnRay;

    public event Action<Ray> OnRightClickPressed;
    public event Action OnRightClickReleased;

    public event Action<Ray> OnLeftClickPressed;
    public event Action OnLeftClickReleased;

    public event Action<Vector2> OnMiddleClickPressed;
    public event Action OnMiddleClickReleased;
    
    public event Action<float> OnZoom;

    void Awake()
    {

        if(playerController == null) playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        playerController.OnAim += AimCheck;
    }

    private void OnDisable()
    {
        playerController.OnAim -= AimCheck;
    }

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        currentMovementRayDelay = movementRayDelay;
        //aim.transform.localPosition = new Vector3(0.0f, -0.5f, 0.0f);
    }

    /// <summary>
    /// Updates the player look direction
    /// </summary>
    public void AimCheck(Vector2 mousePosition)
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Get Ray from mouse position
        currentRay = mainCamera.ScreenPointToRay(mousePosition);
        OnRay?.Invoke(currentRay);

        // Continuous Left Click 
        if (isLeftClickPressed)
        {
            // Check Delay
            if (currentMovementRayDelay > 0.0f)
            {
                //Return if bigger than 0
                currentMovementRayDelay -= Time.deltaTime;
                return;
            }
            else
            {
                // Set delay and continue
                currentMovementRayDelay = movementRayDelay;
                HandleLeftClick();
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
        if (isRightClickPressed) return;
        // bool used to re-trigger on aim change
        isLeftClickPressed = true;
        OnLeftClickPressed?.Invoke(currentRay);
    }

    /// <summary>
    /// Handles the Left Click when released
    /// </summary>
    public void HandleLeftClickUp()
    {
        isLeftClickPressed = false;
        OnLeftClickReleased?.Invoke();
    }

    /// <summary>
    /// Handles the Right Click when pressed
    /// </summary>
    public void HandleRightClick()
    {
        OnRightClickPressed?.Invoke(currentRay);
    }

    /// <summary>
    /// Handles the Right Click when released
    /// </summary>
    public void HandleRightClickUp()
    {
        OnRightClickReleased?.Invoke();
    }

    /// <summary>
    /// Handles the Middle Click when pressed
    /// </summary>
    public void HandleMiddleClick(Vector2 position)
    {
        OnMiddleClickPressed?.Invoke(position);
    }

    /// <summary>
    /// Handles the Middle Click when released
    /// </summary>
    public void HandleMiddleClickUp()
    {
        OnMiddleClickReleased?.Invoke();
    }

    public void HandleZoom(float zoomValue)
    {
        OnZoom?.Invoke(zoomValue);
    }
}

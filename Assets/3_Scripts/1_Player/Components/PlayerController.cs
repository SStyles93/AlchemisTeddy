using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerAction playerActions;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInventoryManager playerInventoryManager;

    public string ControlScheme { get => controlScheme; private set => controlScheme = value; }

    // Private action variables
    private string controlScheme;
    private Vector2 aim;
    private bool pause = false;

    public event Action<Vector2> OnAim;

    private void Awake()
    {
        playerActions = GetComponent<PlayerAction>();
        playerInput = GetComponent<PlayerInput>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
    }

    private void Start()
    {
        controlScheme = playerInput.currentControlScheme;
    }

    public void OnMove(InputAction.CallbackContext value)
    {
        //if (currentLoadingDelay > 0) return;

        //value.ReadValue<Vector2>();
        //Debug.Log($"PlayerControler - OnMove: {value.ReadValue<Vector2>()}");
    }

    public void OnLook(InputAction.CallbackContext value)
    {
        aim = value.ReadValue<Vector2>();
        OnAim?.Invoke(value.ReadValue<Vector2>());
        //Debug.Log($"PlayerControler - OnLook: {aim}");
    }

    public void OnMiddleClick(InputAction.CallbackContext value)
    {
        //if (value.started) Debug.Log($"PlayerControler - OnMiddleClick - Started");
        if (value.performed)
        {
            playerActions.HandleMiddleClick(aim);
            //Debug.Log($"PlayerControler - OnMiddleClick - Performed");
        }
        if (value.canceled)
        {
            playerActions.HandleMiddleClickUp();
            //Debug.Log($"PlayerControler - OnMiddleClick - Canceled");
        }
    }

    public void OnLeftClick(InputAction.CallbackContext value)
    {
        if (playerInput == null) return;
        // Right click takes precedence on left (throw stops movement)

        if (value.performed)
        {
            playerActions.AimCheck(aim);
            playerActions.HandleLeftClick();
            //Debug.Log($"PlayerControler - OnLeftClick - Performed");
        }
        if (value.canceled)
        {
            playerActions.HandleLeftClickUp();
            //Debug.Log($"PlayerControler - OnLeftClick - Canceled");
        }
    }

    public void OnRightClick(InputAction.CallbackContext value)
    {
        if (playerInput == null /*|| currentLoadingDelay > 0*/) return;

        if (value.performed)
        {
            playerActions.HandleRightClick();
            //Debug.Log($"PlayerControler - OnRightClick - Performed");
        }
        if (value.canceled)
        {
            playerActions.HandleRightClickUp();
            //Debug.Log($"PlayerControler - OnRightClick - Canceled");
        }
    }

    public void OnZoom(InputAction.CallbackContext value)
    {
        playerActions.HandleZoom(value.ReadValue<float>());
        //Debug.Log($"Zoom value: {value.ReadValue<float>()}");
    }

    public void OnInventory(InputAction.CallbackContext value)
    {
        //if (currentLoadingDelay > 0) return;

        playerInventoryManager.ToggleInventoryVisibility();
        //Debug.Log($"PlayerControler - OnInventory");
    }

    public void OnPause(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            pause = !pause;
        }
        Time.timeScale = pause ? 0.0f : 1.0f;

        //Debug.Log($"PlayerControler - OnPause");
    }

    /// <summary>
    /// Updates the control scheme (Called by PlayerInput -> Controls Changed Event
    /// </summary>
    public void UpdateControlScheme()
    {
        controlScheme = playerInput.currentControlScheme;
    }
}

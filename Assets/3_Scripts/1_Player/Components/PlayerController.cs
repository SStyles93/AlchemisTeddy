using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerAction playerActions;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInventoryManager playerInventoryManager;
    [SerializeField] private float interactionDelay = 0.25f;
    //[SerializeField] float loadingDelay = 1.5f;


    public string ControlScheme { get => controlScheme; private set => controlScheme = value; }

    // Private action variables
    private string controlScheme;
    private Vector2 aim;
    private Vector2 startRotationPosition;
    private bool isLeftClickPressed = false;
    private bool isMiddleClickPressed = false;
    private bool pause = false;
    private float currentInteractionDelay = 0.25f;
    //private float currentLoadingDelay = 1.5f;

    private void Awake()
    {
        playerActions = GetComponent<PlayerAction>();
        playerCamera = GetComponent<PlayerCamera>();
        playerInput = GetComponent<PlayerInput>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
    }

    private void Start()
    {
        controlScheme = playerInput.currentControlScheme;
        currentInteractionDelay = interactionDelay;
        //currentLoadingDelay = loadingDelay;
    }

    public void Update()
    {
        //if (currentLoadingDelay > 0)
        //{
        //    currentLoadingDelay -= Time.deltaTime;
        //}
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
        playerActions.AimCheck(aim);
        //Debug.Log($"PlayerControler - OnLook: {aim}");
        if (isLeftClickPressed && currentInteractionDelay <= 0.0f)
        {
            playerActions.HandleLeftClick();
            currentInteractionDelay = interactionDelay;
        }
        else
        {
            currentInteractionDelay -= Time.deltaTime;
        }

        if (isMiddleClickPressed)
        {
            if(startRotationPosition == -Vector2.one)
                startRotationPosition = aim;
            else
            {
                float rotationDelta = startRotationPosition.x - aim.x;
                playerCamera.HandleRotation(rotationDelta);
            }
        }
    }

    public void OnMiddleClick(InputAction.CallbackContext value)
    {
        //if (value.started) Debug.Log($"PlayerControler - OnMiddleClick - Started");
        if (value.performed)
        {
            isMiddleClickPressed = true;
            playerCamera.isRotating = true;
            startRotationPosition = -Vector3.one;
            //Debug.Log($"PlayerControler - OnMiddleClick - Performed");
        }
        if (value.canceled)
        { 
            isMiddleClickPressed = false;
            playerCamera.isRotating = false;
            //Debug.Log($"PlayerControler - OnMiddleClick - Canceled");
        }
    }

    public void OnLeftClick(InputAction.CallbackContext value)
    {
        if (playerInput == null /*|| currentLoadingDelay > 0*/) return;

        if (value.performed)
        {
            isLeftClickPressed = true;
            playerActions.AimCheck(aim);
            playerActions.HandleLeftClick();
            //Debug.Log($"PlayerControler - OnLeftClick - Performed");
        }
        if(value.canceled)
        {
            isLeftClickPressed = false;
            playerActions.HandleLeftClickUp();
            //Debug.Log($"PlayerControler - OnLeftClick - Canceled");
        }
    }

    public void OnRightClick(InputAction.CallbackContext value)
    {
        //if (currentLoadingDelay > 0) return;

        //rightClick = value.isPressed;
        //Debug.Log($"PlayerControler - OnRightClick");
    }

    public void OnZoom(InputAction.CallbackContext value)
    {
        playerCamera.HandleZoom(value.ReadValue<float>());
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

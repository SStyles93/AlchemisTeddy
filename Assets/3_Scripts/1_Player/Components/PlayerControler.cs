using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(InputSystem))]
public class PlayerControler : MonoBehaviour
{
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private PlayerActions playerActions;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private PlayerInventoryManager playerInventoryManager;

    public string ControlScheme { get => controlScheme; private set => controlScheme = value; }
    public bool LeftClick { get => leftClick; private set => leftClick = value; }
    public bool RightClick { get => rightClick; private set => rightClick = value; }
    public bool MiddleClick { get => middleClick; private set => middleClick = value; }
    public bool InventoryClick { get => inventoryClick; private set => inventoryClick = value; }
    public bool Pause { get => pause; private set => pause = value; }

    // Private action variables
    private string controlScheme;
    private Vector2 aim;
    private bool leftClick = false;
    private bool rightClick = false;
    private bool middleClick = false;
    private bool inventoryClick = false;
    private bool pause = false;

    private void Awake()
    {
        playerInteraction = GetComponent<PlayerInteraction>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        playerCamera = GetComponent<PlayerCamera>();
        playerActions = GetComponent<PlayerActions>();
    }

    public void OnMove(InputValue value)
    {
        //value.Get<Vector2>();
        Debug.Log($"PlayerControler - OnMove: {value.Get<Vector2>()}");
    }

    public void OnLook(InputValue value)
    {
        aim = value.Get<Vector2>();
        playerActions.AimCheck(aim);
        //Debug.Log($"PlayerControler - OnLook: {value.Get<Vector2>()}");
    }

    public void OnMiddleClick(InputValue value)
    {
        middleClick = value.isPressed;
        Debug.Log($"PlayerControler - OnMiddleClick");
    }

    public void OnLeftClick(InputValue value)
    {
        playerInteraction.HandleLeftClick(aim);
        Debug.Log($"PlayerControler - OnLeftClick at position {aim}");
    }

    public void OnRightClick(InputValue value)
    {
        rightClick = value.isPressed;
        Debug.Log($"PlayerControler - OnRightClick");
    }

    public void OnZoom(InputValue value)
    {
        playerCamera.HandleZoom(value.Get<float>());
        Debug.Log($"Zoom value: {value.Get<float>()}");
    }

    public void OnInventory(InputValue value)
    {
        playerInventoryManager.ToggleInventoryVisibility();
        Debug.Log($"PlayerControler - OnInventory");
    }

    public void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            pause = !pause;
        }
        Time.timeScale = pause ? 0.0f : 1.0f;

        Debug.Log($"PlayerControler - OnPause");
    }
}

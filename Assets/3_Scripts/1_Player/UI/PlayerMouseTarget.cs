using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PlayerMouseTarget : MonoBehaviour
{
    //Reference GameObjects
    [Header("Player's target")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject target;
    [SerializeField] private GraphicRaycaster playerScreenSpaceCanvasRaycaster;
    private Image pointerImage;
    private bool isPointerOverUI = false;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask interactableLayers;



    private void Awake()
    {
        if (target != null) pointerImage = target.GetComponent<Image>();
        if(playerController == null) playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        playerController.OnAim += UpdateTarget;
    }

    private void OnDisable()
    {
        playerController.OnAim -= UpdateTarget;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
    }

    private void UpdateTarget(Vector2 mousePosition)
    {
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
            if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out RaycastHit hit, 100f, interactableLayers))
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
    }

    private bool DetectInteractableUIUnderPointer(Vector2 mousePosition)
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

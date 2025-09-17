using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the visual representation of the player's inventory.
/// It listens to the InventoryManager's OnInventoryChanged event to know when to update.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    //[Header("UI References")]
    //[Tooltip("The parent object that will hold all the inventory slot UI elements.")]
    //[SerializeField] private Transform itemsParent;

    [Tooltip("The inventory pannel that shows the player's items")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject itemsParent;
    [Tooltip("The UI prefab for a single inventory slot.")]
    [SerializeField] private GameObject inventorySlotPrefab;
    
    [SerializeField] private Text titleText;

    // We need a reference to the data source, but only to read from it.
    [Header("Data Source")]
    [SerializeField] private PlayerInventoryManager inventoryManager;

    // State Management
    public bool isSelectionMode { get; private set; } = false;
    private IngredientStation requestingStation;

    // --- Subscribing and Unsubscribing to the Event ---

    private void OnEnable()
    {
        // Subscribe the RedrawUI method to the event.
        // Now, whenever OnInventoryChanged is invoked, RedrawUI will be called automatically.
        PlayerInventoryManager.OnInventoryChanged += RedrawUI;
    }

    private void OnDisable()
    {
        // Unsubscribe when the object is disabled or destroyed.
        // This is crucial to prevent memory leaks and errors if the UI object is destroyed
        // before the InventoryManager.
        PlayerInventoryManager.OnInventoryChanged -= RedrawUI;
    }

    private void Awake()
    {
        if (inventoryManager == null) inventoryManager = GetComponentInParent<PlayerInventoryManager>();
    }

    private void Start()
    {
        inventoryPanel.SetActive(false);
        // Initial drawing of the UI when the game starts.
        RedrawUI();
    }

    /// <summary>
    /// This method is called by the event. It redraws the entire inventory UI.
    /// </summary>
    private void RedrawUI()
    {
        //Debug.Log("Redrawing Inventory UI...");

        // Clear all existing UI slots to prevent duplicates.
        foreach (Transform child in itemsParent.transform)
        {
            Destroy(child.gameObject);
        }

        // Get the current list of items from the manager.
        List<ItemData> currentItems = inventoryManager.GetInventory();

        // Create a new UI slot for each item in the inventory.
        foreach (ItemData item in currentItems)
        {
            GameObject slotInstance = Instantiate(inventorySlotPrefab, itemsParent.transform);

            // Get the InventorySlotUI component from the newly created slot.
            var slotUI = slotInstance.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                // Pass the item data and a reference to the manager to the slot.
                slotUI.Setup(item, inventoryManager, this);
            }
            else
            {
                Debug.LogWarning("InventorySlot prefab is missing an InventorySlotUI component!");
            }
        }
    }

    public void OpenForSelection(IngredientStation station)
    {
        isSelectionMode = true;
        requestingStation = station;
        inventoryPanel.SetActive(true);
        if (titleText != null) titleText.text = "Select an Ingredient";
    }

    public void OnItemSelected(ItemData item)
    {
        if (isSelectionMode && requestingStation != null)
        {
            requestingStation.PlaceItem(item, this.inventoryManager);
            CloseInventory();
        }
    }

    public void CloseInventory()
    {
        isSelectionMode = false;
        requestingStation = null;
        inventoryPanel.SetActive(false);
    }

}

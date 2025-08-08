using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script goes on the Inventory Slot UI prefab. It holds a reference to the inventoryManager
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    private ItemData item;
    private PlayerInventoryManager inventoryManager;
    private InventoryUI inventoryUI;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newItem"></param>
    /// <param name="manager"></param>
    /// <param name="ui"></param>
    public void Setup(ItemData newItem, PlayerInventoryManager manager, InventoryUI ui)
    {
        item = newItem;
        inventoryManager = manager;
        inventoryUI = ui;

        if (transform.TryGetComponent(out Image icon))
        {
            icon.sprite = item.icon;
            icon.enabled = true;
        }
    }

    /// <summary>
    /// Method used by button for when the object is clicked in UI Inventory
    /// </summary>
    public void OnSlotClicked()
    {
        if (item == null || inventoryManager == null || inventoryUI == null) return;

        // Check if we are in selection mode
        if (inventoryUI.isSelectionMode)
        {
            // Check if Item is an Ingredient
            if (item.itemType == ItemType.Ingredient)
            {
                inventoryManager.RemoveItem(item);
                inventoryUI.OnItemSelected(item);
            }
            else
            {
                Debug.Log($"{item.name} is not an ingredient.");
            }
        }
        else
        {
            // Check if the item is a UsableItemData (like a potion).
            if (item is UsableItemData)
            {
                // If it's usable, call the UseItem method.
                inventoryManager.UseItem(item);
            }
        }
    }
}

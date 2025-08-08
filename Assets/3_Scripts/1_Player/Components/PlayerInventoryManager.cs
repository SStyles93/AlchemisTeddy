using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the player's inventory. This script demonstrates safe and efficient
/// coding practices, including dependency management and access modifiers.
/// </summary>
public class PlayerInventoryManager : MonoBehaviour, ISaveable
{
    // --- Good Practice: Cached References & [SerializeField] ---
    // Instead of using Find() or GetComponent() repeatedly in Update(), we assign
    // references once in the Inspector or in Start(). This is far more performant.
    // By making the field private and using [SerializeField], we follow the principle
    // of encapsulation while still allowing designers to link objects in the editor.
    [Header("Dependencies")]
    [Tooltip("The UI Text element used to display status messages.")]
    [SerializeField] private TMPro.TMP_Text statusText;

    [Tooltip("The pannel used to display the Inventory")]
    [SerializeField] private GameObject inventoryPannel = null;
    [SerializeField] private GameObject savePannel = null;

    // The player's inventory is a private list. No other script can directly
    // modify this list, which prevents bugs. They must use public methods like AddItem().
    [Tooltip("A list of all Items in the inventory.")]
    [SerializeField] private List<ItemData> inventory = new List<ItemData>();

    [Tooltip("A list of all possible recipes the player can use.")]
    [SerializeField] private List<CraftingRecipe> availableRecipes;

    public GameObject GetInventoryPannel() => inventoryPannel;
    public List<ItemData> GetInventory() => inventory;
    public List<CraftingRecipe> GetAvailableRecipes() => availableRecipes;

    public static event Action OnInventoryChanged;

    /// <summary>
    /// Toggles the visibility of the Inventory
    /// </summary>
    public void ToggleInventoryVisibility()
    {
        bool visibleState = inventoryPannel.activeSelf;
        visibleState = !visibleState;
        inventoryPannel.SetActive(visibleState);
        savePannel.SetActive(visibleState);
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Adds an item to the inventory and updates the UI.
    /// </summary>
    /// <param name="itemToAdd">The ScriptableObject data of the item to add.</param>
    public void AddItem(ItemData itemToAdd)
    {
        if (itemToAdd == null) return;

        if (itemToAdd is CraftingRecipe recipe)
        {
            if (!availableRecipes.Contains(recipe))
                availableRecipes.Add(recipe);
        }
        else
        {
            if (inventory.Count > 10)
            {
                Debug.Log("Inventory is full");
                return;
            }

            inventory.Add(itemToAdd);
            Debug.Log($"Added {itemToAdd.itemName} to inventory.");

            OnInventoryChanged?.Invoke();

            // Update the status text to show what was added.
        }
        UpdateStatus(itemToAdd);
    }

    /// <summary>
    /// Removes an item from the inventory.
    /// </summary>
    public void RemoveItem(ItemData itemToRemove)
    {
        if (itemToRemove == null) return;

        if (inventory.Remove(itemToRemove))
        {
            Debug.Log($"Removed {itemToRemove.itemName} from inventory.");
            // Also broadcast the event on removal.
            OnInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Uses an item from the inventory, triggering its effect and removing it.
    /// </summary>
    public void UseItem(ItemData itemToUse)
    {
        // Check if the item is actually a "UsableItemData"
        // here, the (name "is" Type name) is called Pattern Matching introduced in C# 7.0
        if (itemToUse is UsableItemData usableItem)
        {
            // Call the item's specific Use() method.
            // We pass 'gameObject' which is the Player
            usableItem.Use(this.gameObject);

            // After using the item, remove it from the inventory.
            RemoveItem(itemToUse);
        }
        else
        {
            Debug.LogWarning($"{itemToUse.name} is not a usable item.");
        }
    }

    /// <summary>
    /// Updates the status text with information about the last added item.
    /// </summary>
    private void UpdateStatus(ItemData item)
    {
        if (statusText == null)
        {
            Debug.LogWarning("Status Text dependency is not set in the Inspector!");
            return;
        }

        // --- Good Practice: Ternary Operator ---
        // A ternary operator is a concise way to write a simple if-else statement.
        // Here, we check if the item is stackable and set the message accordingly.
        // The format is: condition ? value_if_true : value_if_false;
        string stackableMessage = item.isStackable ? "It is stackable." : "It is not stackable.";

        statusText.text = $"Acquired: {item.itemName}. {stackableMessage}";

        // --- Good Practice: switch statement ---
        // A switch statement is cleaner and often more performant than a long
        // if-else if chain when checking a variable against a set of discrete values,
        // like an enum.
        switch (item.itemType)
        {
            case ItemType.Ingredient:
                Debug.Log("This is an ingredient. It can be used for crafting.");
                break; // 'break' exits the switch statement.

            case ItemType.Potion:
                Debug.Log("This is a potion. It can be consumed for an effect.");
                break;

            case ItemType.Key:
                Debug.Log("This is a key. It can be used to unlock something.");
                break;
            case ItemType.Recipe:
                Debug.Log("This is a recipe. It can be used to craft something.");
                break;

            // The 'default' case runs if none of the other cases match.
            // It's good practice to include it to handle unexpected values.
            default:
                Debug.Log("This is a generic item with no special category.");
                break;
        }
    }

    #region ISaveable Implementation

    public Dictionary<string, string> CaptureState()
    {
        // Get a list of all the ItemIDs from the current inventory.
        // The LINQ Select method is a clean, modern way to do this.
        List<string> itemIDs = inventory.Select(item => item.ItemID).ToList();
        
        //We are adding the list of recipes to the items list (since recipes are also items
        itemIDs.AddRange(availableRecipes.Select(item => item.ItemID).ToList());

        // Join the list of IDs into a single string, separated by commas.
        // This is a robust way to store a list of strings in our key-value pair system.
        // Example: "item001,item003,item001,item002"
        string inventoryStateString = string.Join(",", itemIDs);

        // Return the state in the required dictionary format.
        return new Dictionary<string, string>
        {
            { "inventory", inventoryStateString }
        };
    }

    /// <summary>
    /// Restores the inventory's state from the loaded data.
    /// </summary>
    /// <param name="state">The dictionary containing the saved inventory string.</param>
    public void RestoreState(Dictionary<string, string> state)
    {
        // Check if the saved data contains an "inventory" key.
        if (state.TryGetValue("inventory", out string savedInventoryString))
        {
            // Clear the current inventory and recipe list before loading the new one.
            inventory.Clear();
            availableRecipes.Clear();

            // If the saved string is empty, there's nothing to load.
            if (string.IsNullOrEmpty(savedInventoryString))
            {
                OnInventoryChanged?.Invoke(); // Still invoke to update the UI to be empty.
                return;
            }

            // Split the single string back into a list of individual IDs.
            List<string> itemIDs = savedInventoryString.Split(',').ToList();

            // --- Find all ItemData assets in the project ---
            // This is the most complex part. We need a way to map an ID back to an asset.
            // In production we probably would use Unity's Adressables
            var allItems = Resources.FindObjectsOfTypeAll<ItemData>().ToDictionary(item => item.ItemID);

            // Re-populate the inventory list using the loaded IDs.
            foreach (string id in itemIDs)
            {
                if (allItems.TryGetValue(id, out ItemData itemAsset))
                {
                    if( itemAsset is CraftingRecipe recipe) availableRecipes.Add(recipe);
                    else inventory.Add(itemAsset);
                }
                else
                {
                    Debug.LogWarning($"Could not find ItemData asset with ID: {id}");
                }
            }

            Debug.Log($"Inventory loaded with {inventory.Count} items and {availableRecipes.Count} available recipes.");

            // After loading the inventory, broadcast the change to update the UI.
            OnInventoryChanged?.Invoke();
        }
    }
    #endregion

}

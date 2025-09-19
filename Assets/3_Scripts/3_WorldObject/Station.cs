using System.Collections.Generic;
using UnityEngine;

public abstract class Station : WorldObject, IActivatable, ISaveable
{
    [Header("Dependencies")]
    [Tooltip("Reference to the main Inventory UI panel.")]
    [SerializeField] protected InventoryUI inventoryUI;
    [Tooltip("Reference to the player's inventory manager.")]
    [SerializeField] protected PlayerInventoryManager inventoryManager;
    [Tooltip("Reference to the BoxCollider component of the station.")]
    [SerializeField] protected BoxCollider interactableCollider = null;

    [Header("Data")]
    [Tooltip("The item currently placed on this station.")]
    [SerializeField] protected ItemData currentItem = null; // Exposed for debugging

    [Header("Station Parts")]
    [SerializeField] protected GameObject worldItemPosition = null;
    [SerializeField] protected GameObject currentWorldItem = null; // Exposed for debugging


    public ItemData CurrentItem => currentItem;

    private void Awake()
    {
        if (interactableCollider == null)
            interactableCollider = GetComponent<BoxCollider>();
    }

    /// <summary>
    /// Activates the Station and gives it the Player's reference
    /// </summary>
    /// <param name="activator"></param>
    public abstract void Activate(GameObject activator);

    /// <summary>
    /// Places an item on this station.
    /// </summary>
    public abstract void PlaceItem(ItemData item, PlayerInventoryManager placerInventory = null);

    /// <summary>
    /// Adds the current item back to the player's inventory and clears the station.
    /// </summary>
    public virtual void ReturnItemToPlayer(PlayerInventoryManager playerInventory)
    {
        if (currentItem == null) return;

        playerInventory.AddItem(currentItem);
        //Debug.Log($"Returned {currentItem.itemName} to {playerInventory.gameObject.name}.");
        currentItem = null;
        Destroy(currentWorldItem);
    }

    /// <summary>
    /// Removes and returns the item from this station.
    /// </summary>
    public ItemData ClearStation()
    {
        ItemData item = currentItem;
        currentItem = null;
        Destroy(currentWorldItem);
        return item;
    }

    private void OnDrawGizmos()
    {
        if (interactableCollider == null)
            interactableCollider = GetComponent<BoxCollider>();

        Gizmos.color = (currentItem != null) ? Color.cyan : Color.gray;
        Gizmos.DrawWireCube(transform.position + interactableCollider.center, interactableCollider.size);

        if (currentItem != null)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up, currentItem.itemName);
#endif
        }
    }

    #region ISaveable Implementation

    /// <summary>
    /// Captures the state of the ingredient station for saving.
    /// </summary>
    /// <returns>A dictionary containing the ID of the item on the station, or an empty dictionary if there is no item.</returns>
    public Dictionary<string, string> CaptureState()
    {
        var state = new Dictionary<string, string>();

        // Check if there is an item currently on the station.
        if (currentItem != null)
        {
            // If there is, we save its unique ItemID string.
            // We use a clear key like "currentItemId" to know what this data represents.
            state.Add("currentItemId", currentItem.ItemID);
        }
        // If currentItem is null, we simply return an empty dictionary.
        // The absence of the key on load will tell us the station was empty.

        return state;
    }

    /// <summary>
    /// Restores the state of the ingredient station from loaded data.
    /// </summary>
    /// <param name="state">The dictionary containing the saved data.</param>
    public void RestoreState(Dictionary<string, string> state)
    {
        // Check if the loaded data contains a value for our item.
        if (state.TryGetValue("currentItemId", out string savedItemId))
        {
            // If an ID was saved, we need to find the corresponding ItemData asset.
            // This lookup logic should be centralized for efficiency, but for simplicity,
            // we can use Resources.FindObjectsOfTypeAll here.
            var allItems = Resources.FindObjectsOfTypeAll<ItemData>();
            ItemData foundItem = null;
            foreach (var itemAsset in allItems)
            {
                if (itemAsset.ItemID == savedItemId)
                {
                    foundItem = itemAsset;
                    break; // Found the item, no need to search further.
                }
            }

            if (foundItem != null)
            {
                // If we found the matching ItemData asset, place it on the station.)
                PlaceItem(foundItem);
            }
            else
            {
                Debug.LogWarning($"IngredientStation {gameObject.name} could not find an ItemData asset with saved ID: {savedItemId}. Station will be empty.");
                currentItem = null; // Ensure station is empty if item not found.
                if (currentWorldItem != null) Destroy(currentWorldItem);
            }
        }
        else
        {
            // If no ID was found in the save data, it means the station was empty.
            // We ensure the currentItem is null.
            currentItem = null;
            if (currentWorldItem != null) Destroy(currentWorldItem);
        }
    }

    #endregion
}

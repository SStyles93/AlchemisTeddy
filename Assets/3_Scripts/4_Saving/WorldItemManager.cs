using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A singleton manager that keeps track of all active WorldItem objects in the scene.
/// Items register themselves with this manager when they are enabled and unregister
/// when they are disabled.
/// </summary>
public class WorldItemManager : MonoBehaviour
{
    // --- Singleton Pattern Implementation ---
    public static WorldItemManager Instance { get; private set; }

    private void Awake()
    {
        // Standard singleton setup.
        // If an instance already exists and it's not this one, destroy this one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // Otherwise, set the instance to this component.
            Instance = this;
            // Optional: If you want this manager to persist across scene loads.
            // DontDestroyOnLoad(gameObject);
        }
    }
    // --- End of Singleton Pattern ---


    // The list that will hold all the active items.
    // It's private to prevent other scripts from modifying it directly.
    [SerializeField] private List<WorldItem> activeWorldItems = new List<WorldItem>();

    /// <summary>
    /// Adds a WorldItem to the tracked list. Called by the item itself.
    /// </summary>
    public void Register(WorldItem item)
    {
        // Check to prevent adding the same item twice.
        if (!activeWorldItems.Contains(item))
        {
            activeWorldItems.Add(item);
            Debug.Log($"Registered: {item.name}. Total items: {activeWorldItems.Count}");
        }
    }

    /// <summary>
    /// Removes a WorldItem from the tracked list. Called by the item itself.
    /// </summary>
    public void Unregister(WorldItem item)
    {
        if (activeWorldItems.Contains(item))
        {
            activeWorldItems.Remove(item);
            Debug.Log($"Unregistered: {item.name}. Total items: {activeWorldItems.Count}");
        }
    }

    /// <summary>
    /// A public method to get a copy of the list of items.
    /// Returning a new list prevents external scripts from modifying the original list.
    /// </summary>
    public List<WorldItem> GetAllItems()
    {
        // Return a new list to protect the original from external modification.
        return new List<WorldItem>(activeWorldItems);
    }
}

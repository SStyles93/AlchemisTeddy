using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class demonstrates implementing multiple, small interfaces.
/// A treasure chest can be activated (opened) and also damaged (broken).
/// It doesn't need to implement ICollectable, so it isn't forced to.
/// </summary>
public class TreasureChest : MonoBehaviour, IActivatable, IDamageable, ISaveable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    private bool isOpen = false;
    private ItemSpawner itemSpawner = null;

    public event Action<int, int> OnHealthChanged;

    public static event Action<bool> OnChestChangeState;

    private void OnEnable()
    {
        OnChestChangeState += OpenChest;
    }

    private void OnDisable()
    {
        OnChestChangeState -= OpenChest;
    }

    private void Awake()
    {
        if (itemSpawner == null && TryGetComponent<ItemSpawner>(out ItemSpawner spawner))
            itemSpawner = spawner;
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void Activate(GameObject activator)
    {
        if (currentHealth > 0) return;
        OnChestChangeState?.Invoke(isOpen = !isOpen);

        if (items.Count > 0)
        {   // Spawn Items in the items list
            foreach (ItemData item in items)
            {
                itemSpawner.SpawnItem(item);
            }
            items.Clear();
        }
    }

    private void OpenChest(bool value)
    {
        isOpen = value;
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        if (currentHealth <= 0) currentHealth = 0;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"The chest takes {amount} damage. Health is now {currentHealth}.");
    }

    #region ISaveable Implementation

    public Dictionary<string, string> CaptureState()
    {
        List<string> itemIDs = items.Select(item => item.ItemID).ToList();
        string itemStateString = string.Join(",", itemIDs);

        var state = new Dictionary<string, string>
        {
            // Convert all values to strings for serialization.
            { "isOpen", isOpen.ToString() },
            //{ "currentHealth", currentHealth.ToString() }
            { "items", itemStateString }

        };
        return state;
    }

    public void RestoreState(Dictionary<string, string> state)
    {
        if (state.TryGetValue("isOpen", out string isOpenStr))
        {
            // Parse the string back to its original type.
            if(bool.TryParse(isOpenStr, out isOpen))
            OnChestChangeState?.Invoke(isOpen);
        }
        //if (state.TryGetValue("currentHealth", out string healthStr))
        //{
        //    // Parse the string back to its original type.
        //    int.TryParse(healthStr, out currentHealth);
        //}

        // Check if the saved data contains an "item" key.
        if (state.TryGetValue("items", out string savedItemString))
        {
            // Clear the current item list before loading the new one.
            items.Clear();

            // Split the single string back into a list of individual IDs.
            List<string> itemIDs = savedItemString.Split(',').ToList();

            // --- Find all ItemData assets in the project ---
            // This is the most complex part. We need a way to map an ID back to an asset.
            // In production we probably would use Unity's Adressables
            var allItems = Resources.FindObjectsOfTypeAll<ItemData>().ToDictionary(item => item.ItemID);

            // Re-populate the item list using the loaded IDs.
            foreach (string id in itemIDs)
            {
                if (allItems.TryGetValue(id, out ItemData itemAsset))
                {
                    items.Add(itemAsset);
                }
                else
                {
                    Debug.LogWarning($"Could not find ItemData asset with ID: {id}");
                }
            }

        }
    }
    #endregion
}

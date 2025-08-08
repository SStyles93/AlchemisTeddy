using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CraftingTable : MonoBehaviour, IActivatable
{
    [Header("Configuration")]
    [Tooltip("The list of ingredient stations connected to this table.")]
    [SerializeField]
    private List<IngredientStation> ingredientStations;

    // We need a reference to the player's inventory to give them the crafted item.
    [SerializeField] private PlayerInventoryManager playerInventory;
    
    //Item spawner used to spawn WorldItems
    private ItemSpawner itemSpawner;

    private void Awake()
    {
        if (itemSpawner == null && TryGetComponent<ItemSpawner>(out var spawner))
        {
            itemSpawner = spawner;
        }
    }

    /// <summary>
    /// This method will be called when the player activated the table
    /// </summary>
    public void Activate(GameObject activator)
    {
        Debug.Log($"Table Activated by {activator.name}");
        playerInventory = activator.GetComponent<PlayerInventoryManager>();
        Craft();
    }

    /// <summary>
    /// This is the primary method to be called by a UI button or player interaction.
    /// It attempts to craft an item based on the current ingredients.
    /// </summary>
    private void Craft()
    {
        // 1. Gather all ingredients from the connected stations.
        List<ItemData> currentIngredients = new List<ItemData>();
        foreach (var station in ingredientStations)
        {
            if (station.CurrentItem != null)
            {
                currentIngredients.Add(station.CurrentItem);
            }
        }

        if (currentIngredients.Count == 0)
        {
            Debug.Log("No ingredients on the table!");
            return;
        }

        // 2. Find a matching recipe.
        CraftingRecipe matchedRecipe = FindMatchingRecipe(currentIngredients);

        // 3. If a recipe is found, perform the craft.
        if (matchedRecipe != null)
        {
            Debug.Log($"Recipe match found: {matchedRecipe.name}! Crafting {matchedRecipe.outputItem.itemName}.");

            // Consume ingredients from the stations.
            foreach (var station in ingredientStations)
            {
                station.ClearStation();
            }

            // Add the crafted item to the player's inventory.
            if(itemSpawner == null) playerInventory.AddItem(matchedRecipe.outputItem);
            else
                itemSpawner.SpawnItem(matchedRecipe.outputItem);
        }
        else
        {
            Debug.Log("No valid recipe found for the current ingredients.");
        }
    }

    /// <summary>
    /// Checks the provided ingredients against all available recipes.
    /// </summary>
    /// <returns>The matching CraftingRecipe, or null if no match is found.</returns>
    private CraftingRecipe FindMatchingRecipe(List<ItemData> ingredients)
    {
        foreach (var recipe in playerInventory.GetAvailableRecipes())
        {
            // We check if the recipe can be crafted with the ingredients.
            // We also ensure the number of ingredients matches to prevent crafting with extra items on the table.
            if (recipe.ingredients.Count == ingredients.Count && recipe.CanCraft(ingredients))
            {
                return recipe;
            }
        }
        return null;
    }
}

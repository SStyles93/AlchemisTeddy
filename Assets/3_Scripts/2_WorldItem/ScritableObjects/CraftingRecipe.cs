using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Alchemist's Inventory/Crafting Recipe")]
public class CraftingRecipe : ItemData
{
    [Header("Recipe Definition")]
    [Tooltip("The list of ingredients required to craft the item.")]
    public List<ItemData> ingredients;

    [Tooltip("The item that is produced by this recipe.")]
    public ItemData outputItem;

    /// <summary>
    /// Checks if this recipe can be crafted with the provided items.
    /// This version is more robust and handles ingredient order and counts.
    /// </summary>
    public bool CanCraft(List<ItemData> availableIngredients)
    {
        if (ingredients == null || ingredients.Count == 0 || availableIngredients == null)
        {
            return false;
        }

        // This check ensures that the list of available ingredients contains all
        // required ingredients, regardless of order. It also implicitly handles duplicates.
        // For example, if a recipe needs two "Herb" items, the available list must also have at least two.
        return ingredients.All(required => availableIngredients.Count(available => available == required) >= ingredients.Count(req => req == required));
    }
}

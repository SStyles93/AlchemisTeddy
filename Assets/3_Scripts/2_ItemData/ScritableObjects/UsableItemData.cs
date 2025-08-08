using UnityEngine;

/// <summary>
/// LISKOV SUBSTITUTION PRINCIPLE (LSP)
/// This is an abstract base class for any item that can be "used".
/// It inherits from ItemData, adding the concept of an action.
/// Any system that can handle a UsableItemData can handle any of its children
/// (like HealingPotionData or KeyData) without needing to know the difference.
/// </summary>
public abstract class UsableItemData : ItemData
{
    [Header("Usable Item Settings")]
    [Tooltip("The message to display when this item is used successfully.")]
    public string useMessage;

    /// <summary>
    /// This is an abstract method. It has no implementation here.
    /// Any class that inherits from UsableItemData MUST provide its own
    /// implementation for the Use() method.
    /// </summary>
    /// <param name="user">The GameObject that is using the item.</param>
    public abstract void Use(GameObject user);
}

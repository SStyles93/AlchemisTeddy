using UnityEngine;

/// <summary>
/// This is the base ScriptableObject for all items in the game.
/// It contains the core data that every item must have.
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Alchemist's Inventory/Item")]
public class ItemData : ScriptableObject
{
    // By using [Header], we can create categories in the Inspector,
    // making it much easier to read and manage.
    [Header("Core Item Information")]

    // [Tooltip] provides a helpful description when a user hovers over the field name.
    [Tooltip("The name of the item that will be displayed in the UI.")]
    public string itemName;

    // [TextArea] allows for a multi-line string field in the Inspector,
    // which is much better for writing descriptions.
    [Tooltip("The description of the item, shown when the player inspects it.")]
    [TextArea(3, 5)]
    public string description;

    // [Range] constrains a numerical value between a minimum and maximum.
    // This prevents data entry errors, like setting a negative value.
    [Tooltip("The monetary value of the item.")]
    [Range(0, 999)]
    public int value;



    [Space(15)]
    [Header("Item Graphical Settings")]
    [Tooltip("The icon that will represent this item in the inventory.")]
    public Sprite icon;

    [Tooltip("The object that will represent the item in the world")]
    public GameObject prefab;

    [Tooltip("Trail's colors for when the object is spawned \n[0] - Begin of trail\n[1] - End of trail")]
    //Color Usage is to declare HDR colors
    [ColorUsage(true, true)]
    public Color[] trailColors = new Color[2] { Color.white, Color.white };



    // [Space] adds a visual gap in the Inspector, which helps to separate
    // different groups of variables for better clarity.
    [Space(15)]

    [Header("Item Properties")]

    [Tooltip("The category this item belongs to.")]
    public ItemType itemType = ItemType.Generic;

    [Tooltip("Is this item stackable in the inventory?")]
    public bool isStackable = true;

    // This is a private variable. By default, it is not visible in the Inspector.
    private string internalId = System.Guid.NewGuid().ToString();

    // However, by using [SerializeField], we can expose a private variable
    // to the Inspector. This is good practice because it allows other scripts
    // to access this variable only through controlled public methods,
    // while still allowing designers to set its value in the Editor if needed.
    [SerializeField]
    [Tooltip("A unique identifier for this item, used for saving/loading.")]
    private string itemID;

    /// <summary>
    /// Public accessor for the private itemID.
    /// This follows the principle of encapsulation.
    /// </summary>
    public string ItemID => itemID;

    // This method is automatically called by Unity when the script is first created
    // or when the "Reset" command is used in the Inspector.
    // We use it to assign a unique ID automatically.
    private void Reset()
    {
        // We ensure the ID is unique by combining the asset's name and a GUID.
        itemID = $"{this.name}_{System.Guid.NewGuid()}";
    }

    // This method is called whenever a value is changed in the Inspector.
    // It's useful for validating data in real-time.
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(itemID))
        {
            // If the ID is ever cleared by mistake, this will regenerate it.
            Reset();
        }
    }
}

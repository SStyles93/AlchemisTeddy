using UnityEngine;

/// <summary>
/// A concrete implementation of UsableItemData.
/// This object can be substituted anywhere a UsableItemData is expected.
/// </summary>
[CreateAssetMenu(fileName = "New Healing Potion", menuName = "Alchemist's Inventory/Healing Potion")]
public class HealingPotionData : UsableItemData
{
    [Header("Healing Properties")]
    [Tooltip("The total amount of health to restore.")]
    public int healthToRestore;

    [Space]
    [Tooltip("Is this a heal-over-time effect?")]
    public bool isHealOverTime = false;

    [Tooltip("If heal-over-time, how many seconds should the effect last?")]
    [Range(1f, 30f)]
    public float duration = 5f;


    /// <summary>
    /// The concrete implementation of the Use method for this specific item.
    /// </summary>
    public override void Use(GameObject user)
    {
        // Find the health component on the user and heal them.
        if (user.TryGetComponent<PlayerHealth>(out var playerHealth))
        {
            playerHealth.StartHealing(this);
        }
    }
}

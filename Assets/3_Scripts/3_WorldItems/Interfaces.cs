using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This interface defines an object that can be activated.
/// A lever or a button would implement this.
/// </summary>
public interface IActivatable
{
    /// <summary>
    /// Activates the object.
    /// </summary>
    /// <param name="activator">The GameObject of the character performing the activation.</param>
    void Activate(GameObject activator);
}

/// <summary>
/// This interface defines an object that can be collected by the player.
/// </summary>
public interface ICollectable
{
    /// <summary>
    /// Collects the item and adds it to the specified inventory.
    /// </summary>
    /// <param name="collectorInventory">The InventoryManager of the character who is collecting the item.</param>
    void Collect(PlayerInventoryManager collectorInventory);
}

/// <summary>
/// This interface defines an object that can take damage.
/// A breakable crate or an enemy would implement this.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);

    event Action<int, int> OnHealthChanged;

}

public interface ISaveable
{
    /// <summary>
    /// Captures the component's specific state as a dictionary of string key-value pairs.
    /// </summary>
    Dictionary<string, string> CaptureState();

    /// <summary>
    /// Restores the component's state from the provided dictionary.
    /// </summary>
    void RestoreState(Dictionary<string, string> state);
}


using UnityEngine;
using System; // Required for Guid

/// <summary>
/// This component provides a persistent, unique identifier for any GameObject
/// that needs to be saved. It should be attached to any object that will be
/// part of the save system, whether it implements ISaveable or not.
/// The ID is automatically generated in the editor and remains consistent.
/// </summary>
[ExecuteAlways] // This attribute makes the script run in the editor, not just in play mode.
public class SaveableEntity : MonoBehaviour
{
    [Tooltip("The unique ID for this saveable entity. Do not change this manually.")]
    [SerializeField]
    private string uniqueId = "";

    /// <summary>
    /// Public property to access the unique ID from other scripts like the GameManager.
    /// </summary>
    public string UniqueId => uniqueId;

    // The Awake method is called when the script instance is being loaded.
    // Because of [ExecuteAlways], this can happen in the editor.
    private void Awake()
    {
        // We only want to generate a new ID if it's empty. This prevents the ID
        // from changing every time the scene is loaded in the editor.
        if (string.IsNullOrEmpty(uniqueId))
        {
            // Generate a new Globally Unique Identifier (GUID).
            // This creates a long, random string that is statistically guaranteed to be unique.
            uniqueId = Guid.NewGuid().ToString();
        }
    }
}

using System.Collections.Generic;

// --- Main Save File Structure ---

/// <summary>
/// The root container for the entire scene save data.
/// </summary>
[System.Serializable]
public class SceneSaveData
{
    // A list of all the root-level objects in the scene that are saveable.
    public List<GameObjectSaveData> rootObjects;
    public List<WorldItemSaveData> savedWorldItems;
    public SceneSaveData() 
    {
        rootObjects = new List<GameObjectSaveData>(); 
        savedWorldItems = new List<WorldItemSaveData>();
    }
}

/// <summary>
/// Represents the saved state of a single GameObject and its children.
/// </summary>
[System.Serializable]
public class GameObjectSaveData
{
    // Generic Data
    public string uniqueID;
    public string name;
    public bool isActive;

    // Transform Data (using DTOs)
    public Vector3Data position;
    public QuaternionData rotation;
    public Vector3Data scale;

    // Specific Component Data
    // Key: The component's Type name (e.g., "PlayerHealth").
    // Value: The state data for that specific component.
    public Dictionary<string, Dictionary<string, string>> componentSaveData;

    // Hierarchy Data
    public List<GameObjectSaveData> children;

    public GameObjectSaveData()
    {
        componentSaveData = new Dictionary<string, Dictionary<string, string>>();
        children = new List<GameObjectSaveData>();
    }
}

[System.Serializable]
public class WorldItemSaveData
{
    public string itemID; // The ID of the ItemData ScriptableObject
    public Vector3Data position;
    public QuaternionData rotation;
}

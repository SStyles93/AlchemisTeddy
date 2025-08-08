using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Progress;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private IDataService dataService;
    private const string SAVE_FILE_NAME = "scene_save.json";
    private Dictionary<string, SaveableEntity> allEntities;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        dataService = new JsonDataService();
    }

    public void SaveGame()
    {
        var sceneSaveData = new SceneSaveData();

        // --- 1. HIERARCHICAL SAVE (for persistent scene objects) ---
        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in rootObjects)
        {
            if (go.GetComponent<SaveableEntity>() != null)
            {
                sceneSaveData.rootObjects.Add(CaptureStateRecursive(go));
            }
        }

        // --- 2. WORLD ITEM SAVE (for dynamically spawned items) ---
        // Get all tracked items from the manager.
        List<WorldItem> itemsToSave = WorldItemManager.Instance.GetAllItems();
        foreach (WorldItem item in itemsToSave)
        {
            // Create a save data entry for each item.
            var itemSaveData = new WorldItemSaveData
            {
                itemID = item.GetItemData().ItemID,
                position = new Vector3Data(item.transform.position),
                rotation = new QuaternionData(item.transform.rotation)
            };
            sceneSaveData.savedWorldItems.Add(itemSaveData);
        }

        // --- 3. WRITE TO FILE ---
        dataService.Save(sceneSaveData, SAVE_FILE_NAME);
        Debug.Log($"Game Saved. Saved {sceneSaveData.rootObjects.Count} root hierarchies and {sceneSaveData.savedWorldItems.Count} world items.");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Cleans WorldItems, Restores the state of Saveable Entities, Instantiates WorldItems at the position they where when saved
    /// </summary>
    public void LoadGame()
    {
        var sceneSaveData = dataService.Load(SAVE_FILE_NAME);
        if (sceneSaveData == null)
        {
            Debug.Log("No save data found.");
            return;
        }

        // --- 1. CLEANUP: Destroy all currently tracked world items ---
        // This prevents duplicating items when loading. The manager's list will auto-clear
        // as each item's OnDisable() is called during destruction.
        List<WorldItem> existingItems = WorldItemManager.Instance.GetAllItems();
        foreach (WorldItem item in existingItems)
        {
            Destroy(item.gameObject);
        }

        // --- 2. HIERARCHICAL LOAD (for persistent scene objects) ---
        allEntities = FindObjectsByType<SaveableEntity>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID).ToDictionary(e => e.UniqueId);
        foreach (var rootObjectData in sceneSaveData.rootObjects)
        {
            RestoreStateRecursive(rootObjectData);
        }

        // --- 3. WORLD ITEM LOAD (for dynamically spawned items) ---
        // This part requires the system to find the prefab in the ItemData.

        var itemDataLookup = Resources.FindObjectsOfTypeAll<ItemData>()
                                     .ToDictionary(item => item.ItemID);

        foreach (var itemSaveData in sceneSaveData.savedWorldItems)
        {
            GameObject itemPrefab = FindItemPrefabByID(itemSaveData.itemID, itemDataLookup);
            if (itemPrefab != null)
            {
                // Instantiate the new item. Its OnEnable() will automatically register it.
                Instantiate(itemPrefab, itemSaveData.position.ToVector3(), itemSaveData.rotation.ToQuaternion());
            }
            else
            {
                Debug.LogWarning($"Could not find prefab for item ID: {itemSaveData.itemID}");
            }
        }

        Debug.Log("Game Loaded.");
    }

    /// <summary>
    /// Finds an item's prefab by searching through a provided lookup dictionary of all ItemData assets.
    /// </summary>
    /// <param name="itemID">The unique ID of the item to find.</param>
    /// <param name="itemDataLookup">A pre-built dictionary mapping ItemIDs to ItemData assets.</param>
    /// <returns>The found GameObject prefab, or null if not found.</returns>
    private GameObject FindItemPrefabByID(string itemID, Dictionary<string, ItemData> itemDataLookup)
    {
        if (string.IsNullOrEmpty(itemID)) return null;

        // Use the fast dictionary lookup.
        if (itemDataLookup.TryGetValue(itemID, out ItemData itemData))
        {
            // Return the prefab reference from the found ItemData.
            return itemData.prefab;
        }

        // If the ID wasn't in the dictionary, return null.
        return null;
    }

    /// <summary>
    /// Recursively captures the state of a GameObject and all its children.
    /// </summary>
    private GameObjectSaveData CaptureStateRecursive(GameObject go)
    {
        var data = new GameObjectSaveData
        {
            uniqueID = go.GetComponent<SaveableEntity>().UniqueId,
            name = go.name,
            isActive = go.activeSelf,
            position = new Vector3Data(go.transform.position),
            rotation = new QuaternionData(go.transform.rotation),
            scale = new Vector3Data(go.transform.localScale)
        };

        // Capture data from all ISaveable components on this object.
        foreach (var saveable in go.GetComponents<ISaveable>())
        {
            string componentTypeName = saveable.GetType().ToString();
            data.componentSaveData[componentTypeName] = saveable.CaptureState();
        }

        // Recurse into children.
        foreach (Transform child in go.transform)
        {
            // IMPORTANT: We only save children that have a SaveableEntity.
            // This prevents saving parts of complex prefabs you don't care about.
            if (child.GetComponent<SaveableEntity>() != null)
            {
                data.children.Add(CaptureStateRecursive(child.gameObject));
            }
        }
        return data;
    }

    /// <summary>
    /// Recursively restores the state of a GameObject and its children from save data.
    /// </summary>
    private void RestoreStateRecursive(GameObjectSaveData data)
    {
        if (!allEntities.TryGetValue(data.uniqueID, out SaveableEntity entity))
        {
            // This object was in the save file but is no longer in the scene.
            // In a real game, you might want to re-instantiate it from a prefab here.
            return;
        }

        // Restore generic transform and active state.
        entity.gameObject.SetActive(data.isActive);
        entity.transform.position = data.position.ToVector3();
        entity.transform.rotation = data.rotation.ToQuaternion();
        entity.transform.localScale = data.scale.ToVector3();

        // Restore specific component data.
        foreach (var saveable in entity.GetComponents<ISaveable>())
        {
            string componentTypeName = saveable.GetType().ToString();
            if (data.componentSaveData.TryGetValue(componentTypeName, out var componentState))
            {
                saveable.RestoreState(componentState);
            }
        }

        // Recurse into children.
        foreach (var childData in data.children)
        {
            RestoreStateRecursive(childData);
        }
    }
}

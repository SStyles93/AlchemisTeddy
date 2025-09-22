using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    public GameObject Player => player;

    private IDataService dataService;
    private const string PLAYER_SAVE_FILE = "player_save.json";
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerInventoryManager inventoryManager;
    private Dictionary<string, SaveableEntity> allEntities;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        dataService = new JsonDataService();
    }
    
    public void InitializePlayerRef()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    #region SAVING

    // ---------------- SCENE SAVE ----------------
    #region SCENE SAVING
    public void SaveSceneData()
    {
        var sceneSaveData = new SceneSaveData();

        // 1. Save root SaveableEntities
        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in rootObjects)
        {
            if (go.GetComponent<SaveableEntity>() != null)
            {
                sceneSaveData.rootObjects.Add(CaptureStateRecursive(go));
            }
        }

        // 2. Save dynamically spawned world items
        foreach (WorldItem item in WorldItemTracker.Instance.GetAllItems())
        {
            sceneSaveData.savedWorldItems.Add(new WorldItemSaveData
            {
                itemID = item.GetItemData().ItemID,
                position = new Vector3Data(item.transform.position),
                rotation = new QuaternionData(item.transform.rotation)
            });
        }

        // 3. Write file (per scene)
        dataService.Save(sceneSaveData, GetSceneSaveFile());

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

        Debug.Log($"Scene saved to {GetSceneSaveFile()}.");
    }

    public void LoadSceneData()
    {
        var sceneSaveData = dataService.Load<SceneSaveData>(GetSceneSaveFile());
        if (sceneSaveData == null)
        {
            Debug.Log("No scene save found.");
            return;
        }

        // 1. Cleanup world items
        foreach (WorldItem item in WorldItemTracker.Instance.GetAllItems())
        {
            Destroy(item.gameObject);
        }

        // 2. Restore SaveableEntities
        allEntities = FindObjectsByType<SaveableEntity>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                        .ToDictionary(e => e.UniqueId);

        foreach (var rootObjectData in sceneSaveData.rootObjects)
        {
            RestoreStateRecursive(rootObjectData);
        }

        // 3. Restore world items
        var itemDataLookup = Resources.FindObjectsOfTypeAll<ItemData>().ToDictionary(item => item.ItemID);

        foreach (var itemSaveData in sceneSaveData.savedWorldItems)
        {
            GameObject prefab = FindItemPrefabByID(itemSaveData.itemID, itemDataLookup);
            if (prefab != null)
            {
                Instantiate(prefab, itemSaveData.position.ToVector3(), itemSaveData.rotation.ToQuaternion());
            }
            else
            {
                Debug.LogWarning($"Missing prefab for item ID {itemSaveData.itemID}");
            }
        }

        Debug.Log($"Scene {SceneManager.GetActiveScene().name} loaded.");


    }
    #endregion
    // ---------------- PLAYER SAVE ----------------
    #region PLAYER SAVING
    public void SavePlayerData()
    {
        InitializePlayerRef();

        if(player == null)
        {
            Debug.LogWarning($"{this} - SavePlayerData - Player is null");
            return;
        }

        inventoryManager = player?.GetComponent<PlayerInventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogWarning("No PlayerInventoryManager found to save.");
            return;
        }

        PlayerSaveData playerData = new PlayerSaveData
        {
            inventoryItemIDs = inventoryManager.GetInventoryIDs(),
            playerHealth = 100.0f
        };

        dataService.Save(playerData, PLAYER_SAVE_FILE);
        Debug.Log("Player saved.");
    }

    public void LoadPlayerData()
    {
        InitializePlayerRef();
        if(player == null)
        {
            Debug.LogWarning($"{this} - LoadPlayerData - Player is null");
            return;
        }

        var playerData = dataService.Load<PlayerSaveData>(PLAYER_SAVE_FILE);
        if (playerData == null)
        {
            Debug.Log("No player save found.");
            return;
        }

        inventoryManager = player?.GetComponent<PlayerInventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.RestoreFromIDs(playerData.inventoryItemIDs);
        }

        Debug.Log("Player loaded.");
    }
    #endregion
    // ---------------- HELPERS ----------------
    #region HELPERS

    private GameObject FindItemPrefabByID(string itemID, Dictionary<string, ItemData> itemDataLookup)
    {
        if (string.IsNullOrEmpty(itemID)) return null;
        return itemDataLookup.TryGetValue(itemID, out ItemData itemData) ? itemData.prefab : null;
    }

    private GameObjectSaveData CaptureStateRecursive(GameObject go)
    {
        var entityComp = go.GetComponent<SaveableEntity>();
        var data = new GameObjectSaveData
        {
            uniqueID = entityComp.UniqueId,
            name = go.name,
            isActive = go.activeSelf,
            position = new Vector3Data(go.transform.position),
            rotation = new QuaternionData(go.transform.rotation),
            scale = new Vector3Data(go.transform.localScale)
        };

        // Skip PlayerInventoryManager — handled in SavePlayer
        foreach (var saveable in go.GetComponents<ISaveable>())
        {
            // Skip PlayerInventoryManager — handled in SavePlayer
            if (saveable is PlayerInventoryManager) continue;

            data.componentSaveData[saveable.GetType().ToString()] = saveable.CaptureState();
        }

        foreach (Transform child in go.transform)
        {
            if (child.GetComponent<SaveableEntity>() != null)
            {
                data.children.Add(CaptureStateRecursive(child.gameObject));
            }
        }
        return data;
    }

    private void RestoreStateRecursive(GameObjectSaveData data)
    {
        if (!allEntities.TryGetValue(data.uniqueID, out SaveableEntity entity)) return;

        entity.gameObject.SetActive(data.isActive);
        entity.transform.position = data.position.ToVector3();
        entity.transform.rotation = data.rotation.ToQuaternion();
        entity.transform.localScale = data.scale.ToVector3();

        foreach (var saveable in entity.GetComponents<ISaveable>())
        {
            string typeName = saveable.GetType().ToString();
            if (data.componentSaveData.TryGetValue(typeName, out var componentState))
            {
                saveable.RestoreState(componentState);
            }
        }

        foreach (var childData in data.children)
        {
            RestoreStateRecursive(childData);
        }
    }

    private string GetSceneSaveFile()
    {
        return $"{SceneManager.GetActiveScene().name}_scene.json";
    }
    #endregion
    #endregion
}

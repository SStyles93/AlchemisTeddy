using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Collections.Specialized.BitVector32;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }
    private IDataService dataService;
    private const string SESSION_FILE_PREFIX = "session_";

    private SessionSaveData currentSessionData;

    private GameObject currentPlayerInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        dataService = new JsonDataService(); // Or any other IDataService implementation
    }

    // ---------------- SAVE ----------------
    public void SaveSession(string sessionName)
    {
        if (string.IsNullOrEmpty(sessionName))
        {
            Debug.LogError("Session name cannot be empty.");
            return;
        }

        currentSessionData = new SessionSaveData
        {
            sessionID = sessionName,
            timestamp = DateTime.Now,
            currentSceneName = SceneManager.GetActiveScene().name
        };

        // 1. Save Player Data
        CapturePlayerData();

        // 2. Save all loaded scenes data
        for (int i = 0; i < SceneManager.sceneCount;
         i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                currentSessionData.sceneData[scene.name] = CaptureSceneData(scene);
            }
        }

        // 3. Save the session file
        if (dataService.Save(currentSessionData, GetSessionFileName(sessionName), true))
        {
            Debug.Log($"Session \'{sessionName}\' saved successfully.");
        }
        else
        {
            Debug.LogError($"Failed to save session \'{sessionName}\'\n");
        }
    }

    private void CapturePlayerData()
    {
        // Find the player if not already referenced
        if (currentPlayerInstance == null)
        {
            currentPlayerInstance = GameObject.FindGameObjectWithTag("Player");
        }

        if (currentPlayerInstance == null)
        {
            Debug.LogWarning("Player GameObject not found. Player data will not be saved.");
            currentSessionData.playerData = null;
            return;
        }

        PlayerSaveData playerData = new PlayerSaveData();

        // Example: Save inventory (requires PlayerInventoryManager component on player)
        PlayerInventoryManager inventoryManager = currentPlayerInstance.GetComponent<PlayerInventoryManager>();
        if (inventoryManager != null)
        {
            playerData.inventoryItemIDs = inventoryManager.GetInventoryIDs();
        }
        else
        {
            Debug.LogWarning("PlayerInventoryManager not found on player. Inventory will not be saved.");
        }

        // Example: Save player health (requires a Health component on player)
        // Health playerHealthComponent = currentPlayerInstance.GetComponent<Health>();
        // if (playerHealthComponent != null)
        // {
        //     playerData.playerHealth = playerHealthComponent.CurrentHealth;
        // }
        // else
        // {
        //     Debug.LogWarning("Health component not found on player. Health will not be saved.");
        // }

        currentSessionData.playerData = playerData;
    }

    private SceneSaveData CaptureSceneData(Scene scene)
    {
        SceneSaveData sceneSaveData = new SceneSaveData();

        // 1. Save root SaveableEntities
        var rootObjects = scene.GetRootGameObjects();
        foreach (var go in rootObjects)
        {
            SaveableEntity entity = go.GetComponent<SaveableEntity>();
            if (entity != null)
            {
                sceneSaveData.rootObjects.Add(CaptureGameObjectRecursive(go));
            }
        }

        // 2. Save dynamically spawned world items (requires WorldItemTracker)
        if (WorldItemTracker.Instance != null)
        {
            foreach (WorldItem item in WorldItemTracker.Instance.GetAllItems().Where(item => item.gameObject.scene == scene))
            {
                sceneSaveData.savedWorldItems.Add(new WorldItemSaveData
                {
                    itemID = item.GetItemData().ItemID,
                    position = new Vector3Data(item.transform.position),
                    rotation = new QuaternionData(item.transform.rotation)
                });
            }
        }

        return sceneSaveData;
    }

    private GameObjectSaveData CaptureGameObjectRecursive(GameObject go)
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
                data.children.Add(CaptureGameObjectRecursive(child.gameObject));
            }
        }
        return data;
    }

    // ---------------- LOAD ----------------
    public void LoadSession(string sessionName)
    {
        if (string.IsNullOrEmpty(sessionName))
        {
            Debug.LogError("Session name cannot be empty.");
            return;
        }

        SessionSaveData loadedData = dataService.Load<SessionSaveData>(GetSessionFileName(sessionName));
        if (loadedData == null)
        {
            Debug.LogWarning($"No session \'{sessionName}\' found.");
            return;
        }

        currentSessionData = loadedData;

        // Use SceneController for multi-scene loading
        if (SceneController.Instance != null)
        {
            List<string> scenesToLoad = currentSessionData.sceneData.Keys.ToList();
            List<string> scenesToUnload = new List<string>();

            // Determine which currently loaded scenes are NOT in the save data and should be unloaded
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                // Exclude persistent scenes or scenes that are part of the new session
                if (scene.name != "Core" || scene.name != "Session") continue;
                if (scene.isLoaded && !scenesToLoad.Contains(scene.name))
                {
                    scenesToUnload.Add(scene.name);
                }
            }

            // Start the scene transition via SceneController
            StartCoroutine(SceneController.Instance.PerformSessionSceneTransition(
                scenesToLoad,
                scenesToUnload,
                currentSessionData.currentSceneName,
                true, // showOverlay
                true  // clearAssets
            ));
        }
        else
        {
            Debug.LogError("SceneController.Instance is null. Cannot perform session scene transition.");
            // Fallback to simpler loading if SceneController is not available
            StartCoroutine(LoadScenesAdditiveAndRestoreStateFallback(currentSessionData.currentSceneName, currentSessionData.sceneData.Keys.ToList()));
        }
    }

    // Fallback method if SceneController is not available (previous implementation)
    private IEnumerator LoadScenesAdditiveAndRestoreStateFallback(string activeSceneName, List<string> scenesToLoad)
    {
        // 1. Unload all currently loaded scenes except the persistent one (if any)
        List<Scene> loadedScenes = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            loadedScenes.Add(SceneManager.GetSceneAt(i));
        }

        foreach (Scene scene in loadedScenes)
        {
            // Keep the persistent scene (if any)
            if (scene.name != "Core" || scene.name != "Session") continue;
            // and scenes that are part of the new session
            if (scene.isLoaded && !scenesToLoad.Contains(scene.name))
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        // 2. Load all required scenes additively
        foreach (string sceneName in scenesToLoad)
        {
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
        }

        // 3. Set the player\'s scene as the active scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(activeSceneName));

        // 4. Restore the session state
        RestoreSessionStateAfterSceneLoad();

        Debug.Log($"Session \'{currentSessionData.sessionID}\' loaded successfully with multiple scenes (Fallback).");
    }

    // This method is now called after SceneController finishes its transition
    public void RestoreSessionStateAfterSceneLoad()
    {
        if (currentSessionData == null) return; // Should not happen if called correctly

        // 1. Restore Player Data
        RestorePlayerData();

        // 2. Restore all scenes data (only for scenes that were loaded when saved)
        foreach (var sceneEntry in currentSessionData.sceneData)
        {
            Scene scene = SceneManager.GetSceneByName(sceneEntry.Key);
            if (scene.isLoaded)
            {
                RestoreSceneData(scene, sceneEntry.Value);
            }
        }

        Debug.Log($"Session \'{currentSessionData.sessionID}\' loaded successfully.");
    }

    private void RestorePlayerData()
    {
        if (currentSessionData.playerData == null)
        {
            Debug.LogWarning("No player data to restore.");
            return;
        }

        // Instantiate player if not present, or find existing one
        if (currentPlayerInstance == null)
        {
            Debug.LogError("Player prefab not assigned and player not found in scene. Cannot restore player.");
            return;
        }

        // Restore inventory
        PlayerInventoryManager inventoryManager = currentPlayerInstance.GetComponent<PlayerInventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.RestoreFromIDs(currentSessionData.playerData.inventoryItemIDs);
        }

        // Restore health
        // Health playerHealthComponent = currentPlayerInstance.GetComponent<Health>();
        // if (playerHealthComponent != null)
        // {
        //     playerHealthComponent.CurrentHealth = currentSessionData.playerData.playerHealth;
        // }
    }

    private void RestoreSceneData(Scene scene, SceneSaveData sceneSaveData)
    {
        // Clear existing dynamic world items in the scene before restoring
        if (WorldItemTracker.Instance != null)
        {
            foreach (WorldItem item in WorldItemTracker.Instance.GetAllItems().Where(item => item.gameObject.scene == scene).ToList())
            {
                Destroy(item.gameObject);
            }
        }

        // Create a lookup for existing SaveableEntities in the scene by their UniqueId
        Dictionary<string, SaveableEntity> sceneEntities = FindObjectsByType<SaveableEntity>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                                                            .Where(e => e.gameObject.scene == scene)
                                                            .ToDictionary(e => e.UniqueId);

        // Restore root SaveableEntities
        foreach (var rootObjectData in sceneSaveData.rootObjects)
        {
            RestoreGameObjectRecursive(rootObjectData, sceneEntities);
        }

        // Restore dynamically spawned world items
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
                Debug.LogWarning($"Missing prefab for item ID {itemSaveData.itemID} in scene {scene.name}");
            }
        }
    }

    private void RestoreGameObjectRecursive(GameObjectSaveData data, Dictionary<string, SaveableEntity> sceneEntities)
    {
        if (!sceneEntities.TryGetValue(data.uniqueID, out SaveableEntity entity)) return;

        entity.gameObject.SetActive(data.isActive);
        entity.transform.position = data.position.ToVector3();
        entity.transform.rotation = data.rotation.ToQuaternion();
        entity.transform.localScale = data.scale.ToVector3();

        foreach (var saveable in entity.gameObject.GetComponents<ISaveable>())
        {
            // Skip PlayerInventoryManager — handled in SavePlayer
            if (saveable is PlayerInventoryManager) continue;
            saveable.RestoreState(data.componentSaveData[saveable.GetType().ToString()]);
        }

        foreach (var childData in data.children)
        {
            RestoreGameObjectRecursive(childData, sceneEntities);
        }
    }

    private GameObject FindItemPrefabByID(string itemID, Dictionary<string, ItemData> itemDataLookup)
    {
        if (string.IsNullOrEmpty(itemID)) return null;
        return itemDataLookup.TryGetValue(itemID, out ItemData itemData) ? itemData.prefab : null;
    }

    private string GetSessionFileName(string sessionName)
    {
        return $"{SESSION_FILE_PREFIX}{sessionName}.json";
    }

    // Call this method to initialize the player reference if it\'s not set via Inspector
    public void InitializePlayerRef()
    {
        if (currentPlayerInstance == null)
        {
            currentPlayerInstance = GameObject.FindGameObjectWithTag("Player");
        }
    }

    // Public method to get current session data (e.g., for UI display)
    public SessionSaveData GetCurrentSessionData()
    {
        return currentSessionData;
    }

    // Method to list available save sessions
    public IEnumerable<string> ListAvailableSessions()
    {
        // This assumes IDataService.ListSaves() can filter by prefix or that all session files follow the naming convention.
        // A more robust implementation might involve reading metadata from each save file.
        return dataService.ListSaves().Where(fileName => fileName.StartsWith(SESSION_FILE_PREFIX) && fileName.EndsWith(".json"))
                           .Select(fileName => fileName.Replace(SESSION_FILE_PREFIX, "").Replace(".json", ""));
    }

    // This method is called by the SceneController when the scene transition is complete
    public void OnSceneTransitionComplete()
    {
        RestoreSessionStateAfterSceneLoad();
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static SceneController;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }
    public GameObject CurrentPlayerInstance => currentPlayerInstance;

    [SerializeField] GameObject playerPrefab;

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

    private void OnEnable()
    {
        // This method is called by the SceneController when the scene transition is complete
        SceneController.Instance.OnSessionTransitionComplete += RestoreScenesAndPlacePlayer;
        SceneController.Instance.OnSceneLoadComplete += RestoreScene;
    }

    private void OnDisable()
    {
        // This method is called by the SceneController when the scene transition is complete
        SceneController.Instance.OnSessionTransitionComplete -= RestoreScenesAndPlacePlayer;
        SceneController.Instance.OnSceneLoadComplete -= RestoreScene;
    }

    // --------------- CREATE SESSION -------------

    /// <summary>
    /// Creates a temporary SessionSaveData
    /// </summary>
    public void CreateNewSession()
    {
        currentSessionData = new SessionSaveData
        {
            sessionID = "TEMP",
            timestamp = DateTime.Now,
        };
    }

    // ---------------- SAVE ----------------
    public void SaveSession(string sessionID)
    {
        if (string.IsNullOrEmpty(sessionID))
        {
            Debug.LogError("Session name cannot be empty.");
            return;
        }

        currentSessionData = new SessionSaveData
        {
            sessionID = sessionID,
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

        // 3. Save file changes
        SaveFile(sessionID);
    }

    public void SaveScene(string sceneName)
    {
        if (currentSessionData == null) CreateNewSession();

        // 1. Save Player Data
        CapturePlayerData();

        // 2. Save all loaded scenes data
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                currentSessionData.sceneData[scene.name] = CaptureSceneData(scene);
            }
        }

        // 3. Save the session file
        SaveFile(currentSessionData.sessionID);
    }

    public void SaveScene(string sceneName, Transform savedTransform)
    {
        if (currentSessionData == null) CreateNewSession();

        // 1. Save Player Data
        CapturePlayerData();

        // 2. Save all loaded scenes data
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == "Core" || scene.name == "Session") continue;
            if (scene.isLoaded)
            {
                currentSessionData.sceneData[scene.name] = CaptureSceneData(scene);

                // Save the position of the player
                currentSessionData.sceneData[scene.name].playerSavedPosition = new PlayerSavedPosition()
                {
                    position = new Vector3Data(savedTransform.position),
                    rotation = new QuaternionData(savedTransform.rotation),
                    scale = new Vector3Data(savedTransform.localScale)
                };
            }
        }

        // 3. Save the session file
        SaveFile(currentSessionData.sessionID);
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
                if (scene.name == "Core" || scene.name == "Session") continue;
                if (scene.isLoaded && !scenesToLoad.Contains(scene.name))
                {
                    scenesToUnload.Add(scene.name);
                }
            }

            SceneController.Instance.NewTransition()
                .Load(scenesToLoad, currentSessionData.currentSceneName)
                .Unload(scenesToUnload)
                .WithOverlay()
                .WithClearUnusedAssets()
                .Perform();
        }
    }

    // ------------ RESTORE ON CALLBACK ------------

    /// <summary>
    /// Restores all the scenes and places the player in the current one
    /// </summary>
    public void RestoreScenesAndPlacePlayer()
    {
        if (currentSessionData == null) return; // Should not happen if called correctly

        //Instanciate player
        InstantiatePlayer();

        // 1. Restore Player Data
        RestorePlayerData();

        // 2. Restore all scenes data (only for scenes that were loaded when saved)
        foreach (var sceneEntry in currentSessionData.sceneData)
        {
            Scene scene = SceneManager.GetSceneByName(sceneEntry.Key);
            if (scene.name == "Core" || scene.name == "Session") continue;
            if (scene.isLoaded)
            {
                RestoreSceneData(scene, sceneEntry.Value);

                // If player had a saved position place him there
                if (sceneEntry.Value.playerSavedPosition != null)
                {
                    // Set of the player's transform 
                    currentPlayerInstance.transform.SetPositionAndRotation(
                       sceneEntry.Value.playerSavedPosition.position.ToVector3(),
                       sceneEntry.Value.playerSavedPosition.rotation.ToQuaternion());
                    currentPlayerInstance.transform.localScale = sceneEntry.Value.playerSavedPosition.scale.ToVector3();
                    currentPlayerInstance.GetComponent<NavMeshAgent>().Warp(currentPlayerInstance.transform.position);
                }
            }
        }

        // If there is no data for the currently active scene place the player at the "StartPosition" in that scene
        if (!currentSessionData.sceneData.ContainsKey(SceneManager.GetActiveScene().name))
        {
            Transform playerStartTransform = GameObject.FindGameObjectWithTag("PlayerStart").transform;
            currentPlayerInstance.transform.SetPositionAndRotation(playerStartTransform.position, playerStartTransform.rotation);
            currentPlayerInstance.transform.localScale = playerStartTransform.localScale;
            currentPlayerInstance.GetComponent<NavMeshAgent>().Warp(currentPlayerInstance.transform.position);
        }

        Debug.Log($"Session \'{currentSessionData.sessionID}\' loaded successfully.");
    }

    /// <summary>
    /// Restores the state of the loaded scenes
    /// </summary>
    public void RestoreScenes()
    {
        // 2. Restore all scenes data (only for scenes that were loaded when saved)
        foreach (var sceneEntry in currentSessionData.sceneData)
        {
            Scene scene = SceneManager.GetSceneByName(sceneEntry.Key);
            if (scene.name == "Core" || scene.name == "Session") continue;
            if (scene.isLoaded)
            {
                RestoreSceneData(scene, sceneEntry.Value);
            }
        }
    }

    /// <summary>
    /// Restores a given scene
    /// </summary>
    /// <param name="sceneName">Name of the scene to restore</param>
    public void RestoreScene(string sceneName)
    {
        if (!currentSessionData.sceneData.ContainsKey(sceneName)) return;
        var sceneSaveData = currentSessionData.sceneData[sceneName];
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            RestoreSceneData(scene, sceneSaveData);
        }
    }

    // --- HELPERS ---

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
        
        if (scene.name == "Core" || scene.name == "Session") return;

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

    // Public method to get current session data (e.g., for UI display)
    public SessionSaveData GetCurrentSessionData()
    {
        return currentSessionData;
    }

    public SessionSaveData GetSessionFileInfo(string sessionName)
    {
        if (string.IsNullOrEmpty(sessionName))
        {
            Debug.LogError("Session name cannot be empty.");
            return null;
        }

        SessionSaveData loadedData = dataService.Load<SessionSaveData>(GetSessionFileName(sessionName));
        if (loadedData == null)
        {
            Debug.LogWarning($"No session \'{sessionName}\' found.");
            return null;
        }

        return loadedData;
    }

    private string GetSessionFileName(string sessionName)
    {
        return $"{SESSION_FILE_PREFIX}{sessionName}.json";
    }

    // Call this method to initialize the player reference if it\'s not set via Inspector
    public void InstantiatePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            currentPlayerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        else currentPlayerInstance = player;
    }

    // Method to list available save sessions
    public IEnumerable<string> ListAvailableSessions()
    {
        // This assumes IDataService.ListSaves() can filter by prefix or that all session files follow the naming convention.
        // A more robust implementation might involve reading metadata from each save file.
        return dataService.ListSaves().Where(fileName => fileName.StartsWith(SESSION_FILE_PREFIX) && fileName.EndsWith(".json"))
                           .Select(fileName => fileName.Replace(SESSION_FILE_PREFIX, "").Replace(".json", ""));
    }

    private void SaveFile(string sessionID, bool writeOverride = true)
    {
        // 3. Save the session file
        if (dataService.Save(currentSessionData, GetSessionFileName(sessionID), writeOverride))
        {
            Debug.Log($"Session \'{sessionID}\' saved successfully.");
        }
        else
        {
            Debug.LogError($"Failed to save session \'{sessionID}\'\n");
        }
    }
}


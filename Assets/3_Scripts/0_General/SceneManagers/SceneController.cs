using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    #region Singleton
    public static SceneController Instance;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }
    #endregion

    [SerializeField] private LoadingOverlay loadingOverlay;

    // Changed to track scenes by their actual name, as SessionManager will provide names
    private Dictionary<string, List<string>> loadedSceneBySlot = new();
    private bool isBusy = false;

    public event Action OnSceneTransitionComplete;

    // API

    public SceneTransitionPlan NewTransition()
    {
        return new SceneTransitionPlan();
    }

    /// <summary>
    /// Initiates a scene transition based on session data, using the SceneController\'s builder pattern.
    /// </summary>
    /// <param name="scenesToLoadNames">List of scene names to load additively.</param>
    /// <param name="scenesToUnloadNames">List of scene names to unload.</param>
    /// <param name="activeSceneName">The scene that should be set as active after loading.</param>
    /// <param name="showOverlay">Whether to show the loading overlay.</param>
    /// <param name="clearAssets">Whether to clear unused assets after unloading.</param>
    /// <returns>The Coroutine executing the transition plan.</returns>
    public IEnumerator PerformSessionSceneTransition(List<string> scenesToLoadNames, List<string> scenesToUnloadNames, string activeSceneName, bool showOverlay = true, bool clearAssets = true)
    {
        SceneTransitionPlan plan = new SceneTransitionPlan();

        // Unload scenes
        foreach (string sceneName in scenesToUnloadNames)
        {
            plan.Unload(sceneName);
        }

        // Load scenes
        foreach (string sceneName in scenesToLoadNames)
        {

            plan.Load(SceneDatabase.Slots.SessionContent, sceneName);

            if (sceneName == activeSceneName) plan.ActiveScene = activeSceneName; // Ensure active scene is set
        }

        if (showOverlay) plan.WithOverlay();
        if (clearAssets) plan.WithClearUnusedAssets();

        yield return ExecutePlan(plan);
    }

    private IEnumerator ExecutePlan(SceneTransitionPlan plan)
    {
        if (isBusy)
        {
            Debug.LogWarning("Scene change already in progress");
            yield break;
        }
        isBusy = true;
        yield return StartCoroutine(ChangeSceneRoutine(plan));
    }

    private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
    {
        if (plan.Overlay)
        {
            yield return loadingOverlay.FadeInBlack();
        }

        // Unload scenes specified in the plan
        foreach (var sceneName in plan.ScenesToUnload)
        {
            yield return UnloadSceneRoutine(sceneName);
        }

        if (plan.ClearUnusedAssets) yield return CleanupUnusedAssetsRoutine();

        // Load scenes specified in the plan
        foreach (var kvp in plan.ScenesToLoad)
        {
            string sceneSlot = kvp.Key; // Slot
            string sceneName = kvp.Value; // The actual scene name string

            // Check if scene is already loaded by its name
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                // Make sure the dictionary has a list for this slot
                if (!loadedSceneBySlot.ContainsKey(sceneSlot))
                    loadedSceneBySlot[sceneSlot] = new List<string>();

                // Exclusive slots (overwrite)
                if (sceneSlot == SceneDatabase.Slots.Menu || sceneSlot == SceneDatabase.Slots.Session)
                {
                    loadedSceneBySlot[sceneSlot] = new List<string> { sceneName };
                }
                else // Multi-slot: append
                {
                    if (!loadedSceneBySlot[sceneSlot].Contains(sceneName))
                        loadedSceneBySlot[sceneSlot].Add(sceneName);
                }

                if (plan.ActiveScene == sceneName)
                {
                    Scene newScene = SceneManager.GetSceneByName(sceneName);
                    if (newScene.IsValid() && newScene.isLoaded)
                    {
                        SceneManager.SetActiveScene(newScene);
                    }
                }
            }
            else
            {
                yield return LoadAdditiveRoutine(sceneSlot, sceneName, plan.ActiveScene == sceneName);
            }
        }

        // Notify scene transition complete
        OnSceneTransitionComplete?.Invoke();

        if (plan.Overlay)
        {
            yield return loadingOverlay.FadeOutBlack();
        }
        isBusy = false;
    }

    private IEnumerator LoadAdditiveRoutine(string slot, string sceneName, bool setActive)
    {
        // Use sceneName directly for loading, as sceneIndex might not be reliable if not in build settings
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null) yield break;
        loadOp.allowSceneActivation = false;
        while (loadOp.progress < 0.9f)
        {
            yield return null;
        }

        loadOp.allowSceneActivation = true;
        while (!loadOp.isDone)
        {
            yield return null;
        }

        if (setActive)
        {
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid() && newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
            }
        }

        // If the slot is exclusive (only one scene allowed)
        if (slot == SceneDatabase.Slots.Menu || slot == SceneDatabase.Slots.Session)
        {
            loadedSceneBySlot[slot] = new List<string> { sceneName };
        }
        else
        {
            if (!loadedSceneBySlot.ContainsKey(slot))
                loadedSceneBySlot[slot] = new List<string>();

            loadedSceneBySlot[slot].Add(sceneName);
        }
    }

    private IEnumerator UnloadSceneRoutine(string sceneName)
    {
        // Check if the scene is actually loaded before attempting to unload
        Scene sceneToUnload = SceneManager.GetSceneByName(sceneName);
        if (!sceneToUnload.IsValid() || !sceneToUnload.isLoaded) yield break;

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
        if (unloadOp != null)
        {
            while (!unloadOp.isDone)
            {
                yield return null;
            }
        }

        // Remove from loadedSceneBySlot
        foreach (var kvp in loadedSceneBySlot.ToList()) // safe copy for iteration
        {
            if (kvp.Value.Contains(sceneName))
            {
                kvp.Value.Remove(sceneName);

                // If this slot is now empty, remove the slot entry
                if (kvp.Value.Count == 0)
                    loadedSceneBySlot.Remove(kvp.Key);

                break; // scene found, stop
            }
        }
    }

    private IEnumerator CleanupUnusedAssetsRoutine()
    {
        AsyncOperation cleanupOp = Resources.UnloadUnusedAssets();
        while (!cleanupOp.isDone)
        {
            yield return null;
        }
    }

    // Transition Plan Class
    public class SceneTransitionPlan
    {
        // Key is slotName (string), Value is SceneDatabase.Scenes enum (for internal tracking if needed)
        public Dictionary<string, string> ScenesToLoad { get; } = new();
        public List<string> ScenesToUnload { get; } = new(); // List of scene names to unload
        public string ActiveScene;// Renamed to avoid confusion with string name
        public bool ClearUnusedAssets { get; private set; } = false;
        public bool Overlay { get; private set; } = false;
        public bool IsSessionLoad { get; private set; } = false;

        public SceneTransitionPlan Load(string slotName, string scene, bool setActive = false)
        {
            ScenesToLoad[slotName] = scene;
            if (setActive)
            {
                ActiveScene = scene;
            }
            return this;
        }
        public SceneTransitionPlan Unload(string sceneName)
        {
            ScenesToUnload.Add(sceneName);
            return this;
        }
        public SceneTransitionPlan WithOverlay()
        {
            Overlay = true;
            return this;
        }
        public SceneTransitionPlan WithClearUnusedAssets()
        {
            ClearUnusedAssets = true;
            return this;
        }

        public void Perform()
        {
            SceneController.Instance.StartCoroutine(SceneController.Instance.ExecutePlan(this));
        }
    }

    public List<string> GetLoadedScenes()
    {
        List<string> sceneList = new List<string>();
        foreach (string sceneName in loadedSceneBySlot[SceneDatabase.Slots.SessionContent])
        {
            sceneList.Add(sceneName);
        }
        return sceneList;
    }

    // Helper to get scene name from enum (assuming SceneDatabase.Scenes is an enum of scene names)
    private string GetSceneNameFromEnum(SceneDatabase.Scenes sceneEnum)
    {
        return sceneEnum.ToString();
    }

    private SceneDatabase.Scenes GetEnumFromSceneName(string sceneName)
    {
        Enum.TryParse(sceneName, out SceneDatabase.Scenes scene);
        return scene;
    }
}

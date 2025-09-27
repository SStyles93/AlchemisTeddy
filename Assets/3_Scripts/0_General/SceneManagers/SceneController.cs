using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

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
    private Dictionary<string, SceneDatabase.Scenes> loadedSceneBySlot = new();
    private bool isBusy = false;

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
            // Attempt to parse scene name to enum for SceneController\'s internal tracking
            if (System.Enum.TryParse(sceneName, out SceneDatabase.Scenes sceneEnum))
            {
                plan.Load(sceneName, sceneEnum, sceneName == activeSceneName);
            }
            else
            {
                Debug.LogWarning($"Scene \'{sceneName}\' not found in SceneDatabase.Scenes enum. Skipping internal tracking for this scene.");
                // Still add to ScenesToLoad for the actual loading process, but without enum mapping
                plan.ScenesToLoad[sceneName] = SceneDatabase.Scenes.Null; // Use a default/dummy enum value if not found
                if (sceneName == activeSceneName) plan.ActiveSceneStringName = activeSceneName; // Ensure active scene is set
            }
        }

        if (showOverlay) plan.WithOverlay();
        if (clearAssets) plan.WithClearUnusedAssets();

        yield return ExecutePlan(plan.AsSessionLoad());
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
            //yield return new WaitForSeconds(0.5f);
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
            string sceneName = kvp.Key; // The actual scene name string
            SceneDatabase.Scenes sceneEnum = kvp.Value; // The enum value (might be dummy if not found)

            // Check if scene is already loaded by its name
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                // If already loaded, ensure it\'s tracked and potentially set active
                loadedSceneBySlot[sceneName] = sceneEnum;
                if (plan.ActiveSceneStringName == sceneName)
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
                yield return LoadAdditiveRoutine(sceneName, sceneEnum, plan.ActiveSceneStringName == sceneName);
            }
        }

        // If this transition was initiated by the SessionManager, notify it to restore state
        if (plan.IsSessionLoad)
        {
            SessionManager.Instance?.OnSceneTransitionComplete();
        }
        
        if (plan.Overlay)
        {
            yield return loadingOverlay.FadeOutBlack();
        }
        isBusy = false;
    }

    private IEnumerator LoadAdditiveRoutine(string sceneName, SceneDatabase.Scenes sceneEnum, bool setActive)
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
        loadedSceneBySlot[sceneName] = sceneEnum; // Track by name
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
        // Remove from loadedSceneBySlot if it was tracked by its name
        if (loadedSceneBySlot.ContainsKey(sceneName))
        {
            loadedSceneBySlot.Remove(sceneName);
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
        // Key is sceneName (string), Value is SceneDatabase.Scenes enum (for internal tracking if needed)
        public Dictionary<string, SceneDatabase.Scenes> ScenesToLoad { get; } = new();
        public List<string> ScenesToUnload { get; } = new(); // List of scene names to unload
        public SceneDatabase.Scenes ActiveSceneNameEnum { get; private set; } // Renamed to avoid confusion with string name
        public string ActiveSceneStringName { get; set; } // Store string name for comparison
        public bool ClearUnusedAssets { get; private set; } = false;
        public bool Overlay { get; private set; } = false;
        public bool IsSessionLoad { get; private set; } = false;

        public SceneTransitionPlan Load(string sceneName, SceneDatabase.Scenes sceneEnum, bool setActive = false)
        {
            ScenesToLoad[sceneName] = sceneEnum;
            if (setActive)
            {
                ActiveSceneNameEnum = sceneEnum;
                ActiveSceneStringName = sceneName;
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
        public SceneTransitionPlan AsSessionLoad()
        {
            IsSessionLoad = true;
            return this;
        }

        public IEnumerator Perform()
        {
            yield return SceneController.Instance.ExecutePlan(this);
        }
    }

    public void AttributeLoadedScene(string slotKey, int sceneIndex)
    {
        // This method seems to be for tracking scenes loaded by the SceneController itself.
        // If slotKey is now the scene name, this might need adjustment.
        if (System.Enum.IsDefined(typeof(SceneDatabase.Scenes), sceneIndex))
        {
            loadedSceneBySlot[slotKey] = (SceneDatabase.Scenes)sceneIndex;
        }
        else
        {
            Debug.LogWarning($"Invalid sceneIndex {sceneIndex} for slotKey {slotKey}. Not attributing.");
        }
    }

    // Helper to get scene name from enum (assuming SceneDatabase.Scenes is an enum of scene names)
    private string GetSceneNameFromEnum(SceneDatabase.Scenes sceneEnum)
    {
        return sceneEnum.ToString();
    }
}

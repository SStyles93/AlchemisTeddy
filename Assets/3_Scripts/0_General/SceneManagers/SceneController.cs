using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
    }
    #endregion

    [SerializeField] private LoadingOverlay loadingOverlay;

    private Dictionary<string, SceneDatabase.Scenes> loadedSceneBySlot = new();
    private bool isBusy = false;

    // API

    public SceneTransitionPlan NewTransition()
    {
        return new SceneTransitionPlan();
    }

    private Coroutine ExecutePlan(SceneTransitionPlan plan)
    {
        if (isBusy)
        {
            Debug.LogWarning("Scene change already in progress");
            return null;
        }
        isBusy = true;
        return StartCoroutine(ChangeSceneRoutine(plan));
    }
    private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
    {
        if (plan.Overlay)
        {
            yield return loadingOverlay.FadeInBlack();
            yield return new WaitForSeconds(0.5f);
        }
        foreach (var slotKey in plan.ScenesToUnload)
        {
            yield return UnloadSceneRoutine(slotKey);
        }
        if (plan.ClearUnusedAssets) yield return CleanupUnusedAssetsRoutine();

        foreach (var kvp in plan.ScenesToLoad)
        {
            if (loadedSceneBySlot.ContainsKey(kvp.Key))
            {
                yield return UnloadSceneRoutine(kvp.Key);
            }
            yield return LoadAdditiveRoutine(kvp.Key, kvp.Value, plan.ActiveSceneName == kvp.Value);
        }
        if (plan.Overlay)
        {
            yield return loadingOverlay.FadeOutBlack();
        }
        isBusy = false;
    }

    private IEnumerator LoadAdditiveRoutine(string slotKey, SceneDatabase.Scenes sceneName, bool setActive)
    {
        int sceneIndex = (int)sceneName;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
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
            Scene newScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
            if (newScene.IsValid() && newScene.isLoaded)
            {
                // Activate the Scene (make it the current active scene)
                SceneManager.SetActiveScene(newScene);

                // Init the player reference in the SLManager
                SaveLoadManager.Instance.InitializePlayerRef();
                // Load player data
                SaveLoadManager.Instance.LoadPlayerData();
                // Load Scene data
                SaveLoadManager.Instance.LoadSceneData();
            }
        }
        loadedSceneBySlot[slotKey] = sceneName;
    }

    private IEnumerator UnloadSceneRoutine(string slotKey)
    {
        if (!loadedSceneBySlot.TryGetValue(slotKey, out SceneDatabase.Scenes sceneName)) yield break;
        int sceneIndex = (int)sceneName;
        if (sceneIndex == 0) yield break;

        SaveLoadManager.Instance.SaveSceneData();
        SaveLoadManager.Instance.SavePlayerData();

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneIndex);
        if (unloadOp != null)
        {
            while (!unloadOp.isDone)
            {
                yield return null;
            }
        }
        loadedSceneBySlot.Remove(slotKey);
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
        public Dictionary<string, SceneDatabase.Scenes> ScenesToLoad { get; } = new();
        public List<string> ScenesToUnload { get; } = new();
        public SceneDatabase.Scenes ActiveSceneName { get; private set; } = SceneDatabase.Scenes.Core;
        public bool ClearUnusedAssets { get; private set; } = false;
        public bool Overlay { get; private set; } = false;

        public SceneTransitionPlan Load(string slotKey, SceneDatabase.Scenes sceneName, bool setActive = false)
        {
            ScenesToLoad[slotKey] = sceneName;
            if (setActive) ActiveSceneName = sceneName;
            return this;
        }
        public SceneTransitionPlan Unload(string slotKey)
        {
            ScenesToUnload.Add(slotKey);
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
        public Coroutine Perform()
        {
            return SceneController.Instance.ExecutePlan(this);
        }
    }

    public void AttributeLoadedScene(string slotKey, int sceneIndex)
    {
        loadedSceneBySlot[slotKey] = (SceneDatabase.Scenes)sceneIndex;

    }
}

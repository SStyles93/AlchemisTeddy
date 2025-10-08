using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private SceneDatabase.Scenes currentScene;
    [SerializeField] private SceneDatabase.Scenes nextScene;
    [SerializeField] private List<SceneDatabase.Scenes> adjacentScenes = new List<SceneDatabase.Scenes>();
    [SerializeField] private List<SceneDatabase.Scenes> scenesToUndload = new List<SceneDatabase.Scenes>();

    public string CurrentScene => currentScene.ToString();
    public string NextScene => nextScene.ToString();
    public List<string> AdjacentScenes
    {
        get
        {   
            List<string> sceneNames = new List<string>();
            foreach (var scene in adjacentScenes)
            {
                sceneNames.Add(scene.ToString());
            }
            return sceneNames;
        }
    }
    public List<string> ScenesToUndload
    {
        get
        {
            List<string> sceneNames = new List<string>();
            foreach (var scene in scenesToUndload)
            {
                sceneNames.Add(scene.ToString());
            }
            return sceneNames;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If the currenty activated scene is the next scene swap current and next (Invert scenes)
        if (SceneManager.GetActiveScene().buildIndex == (int)nextScene)
        {
            // Swap current & next
            SwapActiveScene();

            // Swap adj.Scenes & scenesToUnload
            SwapAdjacentScenes();

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Save current Scene
        SessionManager.Instance.SaveScene(CurrentScene);

        // Trigger Loading of Adj Scenes & Unload scenesToUnload
        SceneController.Instance?.NewTransition()
            .Load(AdjacentScenes, NextScene)
            .Unload(ScenesToUndload)
            .WithClearUnusedAssets()
            .Perform();

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(NextScene));
    }

    private void OnTriggerExit(Collider other)
    {
        // Set current Scene 
        SwapActiveScene();

        // Swap AjdScenes With ScenesToUnload
        SwapAdjacentScenes();
    }


    /// <summary>
    /// Method used to Swap between the current and next scene
    /// </summary>
    private void SwapActiveScene()
    {
        SceneDatabase.Scenes tmp = nextScene;
        nextScene = currentScene;
        currentScene = tmp;
    }

    private void SwapAdjacentScenes()
    {
        List<SceneDatabase.Scenes> tmpList = adjacentScenes;
        adjacentScenes = scenesToUndload;
        scenesToUndload = tmpList;
    }
}

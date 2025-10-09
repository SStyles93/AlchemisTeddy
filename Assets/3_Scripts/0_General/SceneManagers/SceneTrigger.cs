using System.Collections.Generic;
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

    private Vector3 enterDirection = Vector3.zero;
    private Vector3 exitDirection = Vector3.zero;

    private Vector3 playerPosition;

    private void OnTriggerEnter(Collider other)
    {
        // Save current Scene
        SessionManager.Instance.SaveScene(CurrentScene);
        
        // Get the direction of enter : Pos <- Player
        enterDirection = (transform.position - other.transform.position).normalized;
        enterDirection.y = 0;
        
        //playerPosition = other.transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        // Get the direction of exit
        exitDirection = (other.transform.position - transform.position).normalized;
        exitDirection.y = 0;


        // Player exits from on same side than entrance
        if (Vector3.Dot(enterDirection, exitDirection) < 0.0f) return;

        // Player enters-exits on intended side
        if (Vector3.Dot(transform.forward, exitDirection) > 0.0f)
        {
            ActivateNextScenes();
        }
        // Player enters-exit on opposite side
        else
        {
            SwapActiveScene();
            SwapAdjacentScenes();
            ActivateNextScenes();
            SwapActiveScene();
            SwapAdjacentScenes();
        }
    }

    private void ActivateNextScenes()
    {
        // Trigger Loading of Adj Scenes & Unload scenesToUnload
        SceneController.Instance?.NewTransition()
            .Load(AdjacentScenes, NextScene)
            .Unload(ScenesToUndload)
            .WithClearUnusedAssets()
            .Perform();

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(NextScene));
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

    //private void OnDrawGizmos()
    //{
    //    DrawArrow.ForDebug(playerPosition, enterDirection, Color.green);
    //    DrawArrow.ForDebug(transform.position, exitDirection, Color.red);
    //}
}

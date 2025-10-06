using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private SceneDatabase.Scenes currentScene;
    [SerializeField] private List<SceneDatabase.Scenes> AdjacentScenes = new List<SceneDatabase.Scenes>();
    [SerializeField] private List<SceneDatabase.Scenes> scenesToUndload = new List<SceneDatabase.Scenes>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Check current scene to determin what are the adj/unload Scenes
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Trigger Loading of Adj Scenes
        // Unload scenesToUnload
        // Save current Scene
    }

    private void OnTriggerExit(Collider other)
    {
        // Swap AjdScenes With ScenesToUnload
        // Set current Scene 
    }
}

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


[CreateAssetMenu(fileName = "Portal Orb", menuName = "Alchemist's Inventory/Portal Orb")]
public class PortalOrb : ItemData
{
    [Header("Orb Properties")]
    [Tooltip("The ID of the scene related to the Orb")]
    [SerializeField] private SceneDatabase.Scenes orbScene;
    [Tooltip("The name of the scene to return to")]
    [SerializeField] private List<string> savedScenes;
    private string activeScene = "";

    public string OrbScene { get { return orbScene.ToString();}}
    public List<string> SavedScenes => savedScenes;
    public string ActiveScene => activeScene;

    /// <summary>
    /// Assigns the current scene to return to on the portal orb
    /// </summary>
    /// <param name="currentScene"></param>
    public void AssignActiveScene(string currentScene)
    {
        this.activeScene = currentScene;
    }

    /// <summary>
    /// Adds a Scene to the savedScene list
    /// </summary>
    /// <param name="currentScene"></param>
    public void AddSavedScene(string currentScene)
    {
        if(!this.savedScenes.Contains(currentScene))
        this.savedScenes.Add(currentScene);
    }

    public void AddSavedScenes(List<string> sceneList)
    {
        savedScenes = sceneList;
    }
}

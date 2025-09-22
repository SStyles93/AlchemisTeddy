using UnityEngine;


[CreateAssetMenu(fileName = "Portal Orb", menuName = "Alchemist's Inventory/Portal Orb")]
public class PortalOrb : ItemData
{
    [Header("Orb Properties")]
    [Tooltip("The ID of the scene related to the Orb")]
    [SerializeField] private SceneDatabase.Scenes orbScene;
    [Tooltip("The name of the scene to return to")]
    [SerializeField] private SceneDatabase.Scenes savedScene;

    public SceneDatabase.Scenes OrbScene => orbScene;
    public SceneDatabase.Scenes SavedScene => savedScene;

    /// <summary>
    /// Assigns the current scene to return to on the portal orb
    /// </summary>
    /// <param name="currentScene"></param>
    public void AssignSavedScene(SceneDatabase.Scenes currentScene)
    {
        this.savedScene = currentScene;
    }
}

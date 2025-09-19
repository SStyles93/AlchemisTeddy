using UnityEngine;


[CreateAssetMenu(fileName = "Portal Orb", menuName = "Alchemist's Inventory/Portal Orb")]
public class PortalOrb : ItemData
{
    [Header("Orb Properties")]
    [Tooltip("The ID of the scene related to the Orb")]
    [SerializeField] private int orbScene;
    [Tooltip("The name of the scene to return to")]
    [SerializeField] private int savedScene;

    public int OrbScene => orbScene;
    public int SavedScene => savedScene;

    /// <summary>
    /// Assigns the current scene to return to on the portal orb
    /// </summary>
    /// <param name="currentScene"></param>
    public void AssignSavedScene(int currentScene)
    {
        this.savedScene = currentScene;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script to Visually update the portal and enable clicks to use
/// </summary>
public class Portal : WorldObject, IActivatable
{
    private PortalOrb orbData = null;
    private bool isPortalOpen = false;

    int currentSceneIndex = -1;


    public void Activate(GameObject activator)
    {
        if (isPortalOpen)
        {
            // Trigger "USE" Animation

            //TEMP: will be called by animator
            OnPortalAnimationEnd();
        }
    }

    /// <summary>
    /// Closes the Portal and Nullifies the Orb data
    /// </summary>
    public void Close()
    {
        //TODO:
        //Trigger "Close" animation

        orbData = null;
    }

    /// <summary>
    /// Called from animator and the End of the "Use" animation
    /// </summary>
    public void OnPortalAnimationEnd()
    {
        // If the Orb scene is the current one, send player to saved scene
        if (orbData.OrbScene == currentSceneIndex)
            SceneManager.LoadScene(orbData.SavedScene);
        else
            // Otherwise it means that the player is leaving the Orb Scene
            SceneManager.LoadScene(orbData.OrbScene);
    }

    /// <summary>
    /// Opens the Portal for interaction and transmits the ref of the Orb Data
    /// </summary>
    /// <param name="orbData"></param>
    public void Open(PortalOrb orbData)
    {
        //TODO:
        //Trigger "Open" animation

        //Get ref to OrbData
        this.orbData = orbData;

        // Get the current scene index
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // If the index of the scene to save != the orb's scene index
        // that means that the player is going to the "orb's scene".
        if (currentSceneIndex != orbData.OrbScene)
            // In that case we assign the scene to return to
            orbData.AssignSavedScene(currentSceneIndex);
    }

    public void OnOpenAnimationEnd()
    {
        isPortalOpen = true;
    }

    public void OnCloseAnimationEnd()
    {
        isPortalOpen = false;
    }
}

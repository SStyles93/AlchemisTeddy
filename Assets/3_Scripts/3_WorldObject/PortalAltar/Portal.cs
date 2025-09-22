using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Script to Visually update the portal and enable clicks to use
/// </summary>
public class Portal : WorldObject, IActivatable
{
    //Animation Settings
    [Header("Animation Settings")]
    [SerializeField] Animator animator;

    [Header("Scene transition settings")]
    [SerializeField] SceneDatabase.Scenes nextScene = SceneDatabase.Scenes.ForestScene;

    SceneDatabase.Scenes currentScene = SceneDatabase.Scenes.Core;

    int OpenHash = -1;
    int CloseHash = -1;

    private GameObject player;
    private PortalOrb orbData = null;
    private bool isPortalOpen = false;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        OpenHash = Animator.StringToHash("Open");
        CloseHash = Animator.StringToHash("Close");
    }

    public void Activate(GameObject activator)
    {
        player = activator;
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
        //Launches the Close Animation
        animator.SetTrigger(CloseHash);

        orbData = null;
    }

    /// <summary>
    /// Called from animator and the End of the "Use" animation
    /// </summary>
    public void OnPortalAnimationEnd()
    {

        ////Disable player action before loading next scene (Removes possible Callback errors)
        player.GetComponent<PlayerInput>().enabled = false;
        player.GetComponent<PlayerController>().enabled = false;

        // If the Orb scene is the current one, the player is "in the orb" -> send player to saved scene
        if (orbData.OrbScene == currentScene)
        {
            //Transition to Saved scene
            SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, orbData.SavedScene, true)
            .Unload(SceneDatabase.Slots.Session)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();

        }
        // Otherwise it means that the player is going to Orb Scene 
        else
        {
            SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, orbData.OrbScene, true)
            .Unload(SceneDatabase.Slots.Session)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
        }
            
    }

    /// <summary>
    /// Opens the Portal for interaction and transmits the ref of the Orb Data
    /// </summary>
    /// <param name="orbData"></param>
    public void Open(PortalOrb orbData)
    {
        //Launches the Open Animation
        animator.SetTrigger(OpenHash);

        //Get ref to OrbData
        this.orbData = orbData;

        // Get the current scene index
        currentScene = (SceneDatabase.Scenes)SceneManager.GetActiveScene().buildIndex;
        // If the index of the scene to save != the orb's scene index
        // that means that the player is going to the "orb's scene".
        if (currentScene != orbData.OrbScene)
            // In that case we assign the scene to return to
            orbData.AssignSavedScene(currentScene);
        
        // If no scene it set, fallback to portal's next scene
        if(orbData.SavedScene == SceneDatabase.Scenes.Core)
            orbData.AssignSavedScene(nextScene);
    }

    //Called by the Open Animation
    public void OnOpenAnimationEnd()
    {
        isPortalOpen = true;
    }

    //Called by the Close Animation
    public void OnCloseAnimationEnd()
    {
        isPortalOpen = false;
    }
}

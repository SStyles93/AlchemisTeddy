using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
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
    [SerializeField] List<SceneDatabase.Scenes> nextScenes = new List<SceneDatabase.Scenes>();
    [SerializeField] Transform playerPostition;

    public List<string> NextScenes
    {
        get
        {
            List<string> nextScenesNames = new List<string>();
            foreach (var scene in nextScenes)
            {
                nextScenesNames.Add(scene.ToString());
            }
            return nextScenesNames;
        }
    }

    string currentScene = "";

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
        player.GetComponent<NavMeshAgent>().isStopped = true;

        SessionManager.Instance?.SaveScene(SceneManager.GetActiveScene().name, playerPostition);

        // Player going OUT of ORB
        if (orbData.OrbScene == currentScene)
        {
            orbData.AddSavedScenes(NextScenes);
            orbData.AssignActiveScene(NextScenes[0]);

            //Transition to Saved scene
            SceneController.Instance?.NewTransition()
                .Load(orbData.SavedScenes, orbData.ActiveScene)
                .Unload(currentScene)
                .WithOverlay()
                .WithClearUnusedAssets()
                .WithTransition()
                .Perform();
        }
        // Player going IN ORB
        else
        {
            // The player is going to the "orb's scene".
            if (currentScene != orbData.OrbScene)
            {
                // Assign scene to return to
                orbData.AssignActiveScene(currentScene);
                // Assign all the loaded scenes
                orbData.AddSavedScenes(SceneController.Instance.GetLoadedScenes());
            }

            // Transition to OrbScene
            SceneController.Instance?.NewTransition()
             .Load(orbData.OrbScene)
             .Unload(orbData.SavedScenes)
             .WithOverlay()
             .WithClearUnusedAssets()
             .WithTransition()
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
        currentScene = SceneManager.GetActiveScene().name;


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

    private void OnDrawGizmosSelected()
    {
        DrawArrow.ForGizmo(
            playerPostition.position,
            (playerPostition.transform.position + playerPostition.transform.forward) - playerPostition.transform.position,
            Color.green,
            .25f, 20, 0.05f);

    }

}

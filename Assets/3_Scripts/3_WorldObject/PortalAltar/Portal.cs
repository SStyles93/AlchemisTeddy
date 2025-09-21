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

    int OpenHash = -1;
    int CloseHash = -1;

    private PortalOrb orbData = null;
    private bool isPortalOpen = false;

    int currentSceneIndex = -1;

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
        GameManager.Instance.SavePlayer();
        GameManager.Instance.SaveScene();

        //Disable player action before loading next scene (Removes possible Callback errors)
        GameManager.Instance.Player.GetComponent<PlayerInput>().enabled = false;
        GameManager.Instance.Player.GetComponent<PlayerController>().enabled = false;

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
        //Launches the Open Animation
        animator.SetTrigger(OpenHash);

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

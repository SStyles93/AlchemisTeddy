using System.Collections.Generic;
using UnityEngine;

public class SavedGameManager : MonoBehaviour
{
    //List of Saved Sessions
    // TODO: List the actuall SavedSessionData(s)
    [SerializeField] private List<string> savedSessions = new();

    private void Awake()
    {
        PopulateLoadMenu();
    }

    public void PopulateLoadMenu()
    {
        var sessions = SessionManager.Instance.ListAvailableSessions();
        foreach (var session in sessions)
        {
            savedSessions.Add(session);
        }
        
        // TODO: Retreive actuall info from session
    }

    

    public void ReturnToMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    public void StartSession()
    {
        // Load a session
    }

    // TODO: Get the data and return a "Slot"
    private void RetreiveData()
    {

    }

}

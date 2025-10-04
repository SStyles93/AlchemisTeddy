using System;
using System.Collections.Generic;
using UnityEngine;

public class SavedGameManager : MonoBehaviour
{
    //List of Saved Sessions
    // TODO: List the actuall SavedSessionData(s)
    [SerializeField] private List<string> savedSessions = new();
    [SerializeField] private GameObject sessionSlotHolder;
    [SerializeField] private GameObject sessionsSlotPrefab;

    private void Awake()
    {
        PopulateLoadMenu();
    }

    public void PopulateLoadMenu()
    {
        ClearSessionSlots();

        var sessions = SessionManager.Instance.ListAvailableSessions();
        foreach (var session in sessions)
        {
            savedSessions.Add(session);
            SessionSaveData sessionData = SessionManager.Instance.GetSessionFileInfo(session);

            GameObject currentSessionSlot = Instantiate(sessionsSlotPrefab, sessionSlotHolder.transform);
            currentSessionSlot.transform.GetComponent<SessionSlot>().InitializeSessionSlot(sessionData);
        }
    }

    /// <summary>
    /// Clears all the SessionSlots contained in the SessionSlotHolder
    /// </summary>
    private void ClearSessionSlots()
    {
        //Clear sessionSlots before setup
        int sessionSlotCount = sessionSlotHolder.transform.childCount;
        if (sessionSlotCount > 0)
        {
            for (int i = 0; i < sessionSlotCount; i++)
            {
                Destroy(sessionSlotHolder.transform.GetChild(i).gameObject);
            }
        }
    }

    public void ReturnToMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu.ToString())
            .Unload(SceneDatabase.Scenes.SavedGamesMenu.ToString())
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

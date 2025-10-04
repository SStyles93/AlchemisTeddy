using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void StartNewSession()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session.ToString())
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.LabScene.ToString())
            .Unload(SceneDatabase.Scenes.MainMenu.ToString())
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    public void OpenSavedGamesMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session.ToString())
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SavedGamesMenu.ToString())
            .Unload(SceneDatabase.Scenes.MainMenu.ToString())
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

}

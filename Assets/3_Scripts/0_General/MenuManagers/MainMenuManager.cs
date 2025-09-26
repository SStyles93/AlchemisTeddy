using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void StartNewSession()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.LabScene)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    public void OpenSavedGamesMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SavedGamesMenu)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

}

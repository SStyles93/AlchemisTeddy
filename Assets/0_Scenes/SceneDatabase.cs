using UnityEngine;

public class SceneDatabase : MonoBehaviour
{
    public class Slots
    {
        public const string Menu = "Menu";

        //The Game Session the player is using
        public const string Session = "Session";

        //The content (level the player is loaded in)
        public const string SessionContent = "SessionContent";
    }

    public enum Scenes
    {
        Null = -1,
        Core,
        MainMenu,
        SavedGamesMenu,
        Session,
        LabScene,
        LabCave,
        ForestScene,
        Graveyard
    }
}

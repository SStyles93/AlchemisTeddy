using UnityEngine;

public class SceneDatabase : MonoBehaviour
{
    public class Slots
    {
        public const string Menu = "Menu";

        public const string Session = "Session";

        public const string SessionContent = "SessionContent";
    }

    public enum Scenes
    {
        Core,
        MainMenu,
        LabScene,
        ForestScene
    }
}

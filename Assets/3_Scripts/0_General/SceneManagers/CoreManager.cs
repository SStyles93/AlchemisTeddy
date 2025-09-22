using UnityEngine;

public class CoreManager : MonoBehaviour
{
    void Start()
    {
        // Load everything like AudioManagers, Save System, ...
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.LabScene)
            .Perform();
    }
}

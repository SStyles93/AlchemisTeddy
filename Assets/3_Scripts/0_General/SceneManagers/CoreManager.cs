using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreManager : MonoBehaviour
{
    [SerializeField] private bool isInEditMode = true;

    void Start()
    {
        if (isInEditMode)
        {
            SceneController.Instance
                .AttributeLoadedScene(SceneDatabase.Slots.Session, SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            // Load everything like AudioManagers, Save System, ...
            SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.LabScene, true)
                .Perform();
        }
    }
}

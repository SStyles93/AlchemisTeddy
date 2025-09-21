using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.LoadPlayer();
        GameManager.Instance.LoadScene();
    }
}

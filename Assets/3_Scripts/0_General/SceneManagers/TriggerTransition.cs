using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerTransition : MonoBehaviour
{
    [Header("Scene transition settings")]
    [SerializeField] SceneDatabase.Scenes nextScene = SceneDatabase.Scenes.Null;
    public string NextScene { get => nextScene.ToString(); }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            if (nextScene != SceneDatabase.Scenes.Null)
            {
                player.GetComponent<PlayerInput>().enabled = false;
                player.GetComponent<PlayerController>().enabled = false;
                SessionManager.Instance?.LoadScene(NextScene);

            }
        }
    }
}

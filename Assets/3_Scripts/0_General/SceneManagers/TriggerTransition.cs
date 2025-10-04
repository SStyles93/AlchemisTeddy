using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
                SessionManager.Instance?.SaveScene(SceneManager.GetActiveScene().name);
                player.GetComponent<PlayerInput>().enabled = false;
                player.GetComponent<PlayerController>().enabled = false;
                SessionManager.Instance?.LoadScene(NextScene);

            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawArrow.ForGizmo(transform.localPosition + Vector3.forward, transform.position - (transform.localPosition + Vector3.forward) , Color.blueViolet);
    }
}

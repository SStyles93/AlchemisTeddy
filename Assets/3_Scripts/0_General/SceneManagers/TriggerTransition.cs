using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TriggerTransition : MonoBehaviour
{
    [Header("Scene transition settings")]
    [SerializeField] SceneDatabase.Scenes nextScene = SceneDatabase.Scenes.Null;
    public string NextScene { get => nextScene.ToString(); }

    [SerializeField] Transform playerPostition;

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
                SessionManager.Instance?.SaveScene(SceneManager.GetActiveScene().name, playerPostition);
                player.GetComponent<PlayerInput>().enabled = false;
                player.GetComponent<PlayerController>().enabled = false;
                SessionManager.Instance?.LoadScene(NextScene);

            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawArrow.ForGizmo(
            transform.localPosition + transform.forward, 
            transform.position - (transform.localPosition + transform.forward), 
            Color.blueViolet,
            .5f,20,0.05f);

        DrawArrow.ForGizmo(
            playerPostition.position, 
            (playerPostition.transform.position + playerPostition.transform.forward) -  playerPostition.transform.position,
            Color.green,
            .25f, 20, 0.05f);

    }
}

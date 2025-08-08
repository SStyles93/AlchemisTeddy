using UnityEngine;

public class HUDCanvas : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private void Awake()
    {
        if(player == null)
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);

        Vector3 cameraOpposite = transform.position - (Camera.main.transform.position - transform.position);
        transform.LookAt(cameraOpposite);
    }
}

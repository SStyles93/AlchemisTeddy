using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerThrow : MonoBehaviour
{
    [SerializeField] private PlayerAction playerAction;

    [SerializeField] private LineRenderer lineRenderer;
    [Range(5, 250)]
    [SerializeField] private int linePoints = 25;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        playerAction = GetComponent<PlayerAction>();
    }

    private void OnEnable()
    {
        playerAction.OnRightClickPressed += EnableThrowLine;
        playerAction.OnRightClickReleased += DisableThrowLine;
        playerAction.OnRay += UpdateThrowLine;
    }

    private void OnDisable()
    {
        playerAction.OnRightClickPressed -= EnableThrowLine;
        playerAction.OnRightClickReleased -= DisableThrowLine;
        playerAction.OnRay -= UpdateThrowLine;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateThrowLine(Ray ray)
    {

    }

    private void EnableThrowLine(Ray ray)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = linePoints;

        //Start animation of throw && Block at a certain point
    }

    private void DisableThrowLine()
    {
        lineRenderer.enabled = false;
    }
}

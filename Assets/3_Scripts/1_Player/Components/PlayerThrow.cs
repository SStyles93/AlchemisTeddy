using NUnit;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerThrow : MonoBehaviour
{
    [SerializeField] private PlayerAction playerAction;
    [SerializeField] private LayerMask throwMask;

    [SerializeField] private LineRenderer lineRenderer;
    [Range(5, 250)]
    [SerializeField] private int linePoints = 25;
    [SerializeField] private float arcHeight = 1f;

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

    //Method called by the animator when throw is ready
    public void StartDraw()
    {

    }

    private void UpdateThrowLine(Ray ray)
    {
        if (!lineRenderer.enabled) return;
        DrawThrowLine(ray);
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

    private void DrawThrowLine(Ray ray)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, throwMask))
        {
            endPosition = hitInfo.point;

            for (int i = 0; i <= linePoints - 1; i++)
            {
                // Calculate the 't' value (normalized progress) for this step.
                float t = (float)i / linePoints;

                // Use the same math as the coroutine to calculate the point on the arc.
                Vector3 linearPosition = Vector3.Lerp(startPosition, endPosition, t);
                float arc = 4 * arcHeight * (t - (t * t));
                Vector3 currentPoint = linearPosition + new Vector3(0, arc, 0);

                lineRenderer.SetPosition(i, currentPoint);
            }
        }
    }
}

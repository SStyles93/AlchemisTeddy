using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class PlayerThrow : MonoBehaviour
{
    [Header("Player Component Reference")]
    [SerializeField] private PlayerAction playerAction;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Throw Settings")]
    [SerializeField] private GameObject prefabToThrow;
    [SerializeField] private LayerMask throwMask;
    [SerializeField] private Gradient validGradient;
    [Range(5, 250)]
    [SerializeField] private int linePoints = 25;
    [SerializeField] private float arcHeight = 1f;
    [SerializeField] private float travelDuration = 1f;

    [Header("Character Settings")]
    [SerializeField] Transform handTransform;
    [SerializeField] private float rotationSpeed = 5;

    //Private vars
    Vector3 endPosition;
    Ray targetRay;
    GameObject currentHeldObject = null;
    bool isThrowing = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        playerAction = GetComponent<PlayerAction>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        playerAction.OnRightClickPressed += StartThrowAction;
        playerAction.OnRightClickReleased += StopThrowAction;

        playerAction.OnRay += UpdateThrowLine;
    }

    private void OnDisable()
    {
        playerAction.OnRightClickPressed -= StartThrowAction;
        playerAction.OnRightClickReleased -= StopThrowAction;

        playerAction.OnRay -= UpdateThrowLine;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (currentHeldObject != null) FaceThrowDirection();
    }

    private void StartThrowAction(Ray ray)
    {
        isThrowing = true;
        navMeshAgent.SetDestination(transform.position);
        navMeshAgent.isStopped = true;
        targetRay = ray;

        currentHeldObject = Instantiate(prefabToThrow, handTransform.position, handTransform.rotation, handTransform);
        currentHeldObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    //Method called by the animator when throw is ready
    public void EnableThrowLine()
    {
        if (!isThrowing) return;
        lineRenderer.enabled = true;
        lineRenderer.positionCount = linePoints;
        DrawThrowLine(targetRay);
    }
    private void UpdateThrowLine(Ray ray)
    {
        if (!lineRenderer.enabled) return;
        targetRay = ray;
        DrawThrowLine(targetRay);
    }

    // Method called by the Animator
    public void ThrowAction()
    {
        lineRenderer.enabled = false;

        if (currentHeldObject != null)
        {
            currentHeldObject.transform.SetParent(null, true);
            currentHeldObject.GetComponent<Rigidbody>().isKinematic = false;
            StartCoroutine(MoveAlongArcRoutine(currentHeldObject.transform, handTransform.position, endPosition));
            currentHeldObject = null;
        }
    }

    private void StopThrowAction()
    {
        isThrowing = false;

        navMeshAgent.isStopped = false;
        lineRenderer.enabled = false;
        if (currentHeldObject != null) Destroy(currentHeldObject.gameObject);
    }

    private void DrawThrowLine(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, throwMask))
        {
            endPosition = hitInfo.point;

            for (int i = 0; i <= linePoints - 1; i++)
            {
                // Calculate the 't' value (normalized progress) for this step.
                float t = (float)i / linePoints;

                // Use the same math as the coroutine to calculate the point on the arc.
                Vector3 linearPosition = Vector3.Lerp(handTransform.position, endPosition, t);
                float arc = 4 * arcHeight * (t - (t * t));
                Vector3 currentPoint = linearPosition + new Vector3(0, arc, 0);

                lineRenderer.SetPosition(i, currentPoint);
            }

            lineRenderer.colorGradient = validGradient;
        }
    }

    private void FaceThrowDirection()
    {
        if (navMeshAgent.velocity.sqrMagnitude > 0.01f) return;

        Vector3 direction = endPosition - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// A coroutine that animates a Transform along a parabolic arc.
    /// </summary>
    private IEnumerator MoveAlongArcRoutine(Transform objectToMove, Vector3 start, Vector3 end)
    {
        //Improvement: let the item hold this logic, just call it here
        TrailRenderer objectTrail = objectToMove.GetComponentInChildren<TrailRenderer>();
        if (objectTrail != null) objectTrail.enabled = true;

        float elapsedTime = 0f;

        while (elapsedTime < travelDuration)
        {
            if (objectToMove == null) yield break;

            float t = elapsedTime / travelDuration;
            Vector3 linearPosition = Vector3.Lerp(start, end, t);
            float arc = 4 * arcHeight * (t - (t * t));
            Vector3 arcPosition = linearPosition + new Vector3(0, arc, 0);

            objectToMove.position = arcPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (objectToMove != null)
        {
            objectToMove.position = end;

            //Improvement: let the item hold this logic, just call it here
            if (objectTrail != null) objectTrail.enabled = false;
        }
    }
}

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

public class Barrel : Moveable
{
    [SerializeField] private float movementSpeed = 1.0f;

    private SplineAnimate splineAnimate;
    private float splineLenght;
    private float distancePercentage;



    private new void Awake()
    {
        base.Awake();
        splineAnimate = GetComponent<SplineAnimate>();
    }

    private void Start()
    {
        splineLenght = splineAnimate.Container.CalculateLength();
    }

    public override void Move(Vector3 position)
    {
        // Logic of start placement is held in the base (Moveable class)
        if (!isPlayerPlaced)
        {
            StartPlacement(position);
            return;
        }

        if (!PlayerIsFacing()) return;

        Vector3 correctedPosition = ComputeOppositePosition(position);

        // Move north
        if (position.z > transform.position.z)
        {
            distancePercentage += Time.deltaTime * movementSpeed / splineLenght;
            if (distancePercentage >= 1) distancePercentage = 1;
            transform.position = splineAnimate.Container.EvaluatePosition(distancePercentage);
            player.GetComponent<NavMeshAgent>().SetDestination(correctedPosition);
        }
        // Move south
        if (position.z < transform.position.z)
        {
            distancePercentage -= Time.deltaTime * movementSpeed / splineLenght;
            if (distancePercentage <= 0) distancePercentage = 0;
            transform.position = splineAnimate.Container.EvaluatePosition(distancePercentage);
            player.GetComponent<NavMeshAgent>().SetDestination(correctedPosition);
        }
    }
}

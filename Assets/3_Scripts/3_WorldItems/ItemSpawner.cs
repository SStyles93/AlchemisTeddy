// C#
using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// A component that can be attached to any GameObject to give it the ability
/// to spawn items along a customizable parabolic arc. Includes editor Gizmos
/// for easy setup and visualization.
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [Header("Spawning Configuration")]
    [Tooltip("The data of the item to spawn. Must contain a prefab.")]
    [SerializeField] private ItemData itemToSpawn;

    [Tooltip("The transform where the item should be spawned and the arc should begin.")]
    [SerializeField] private Transform spawnPoint;
    [Tooltip("The transform where the item should be landing and the arc should end.")]
    [SerializeField] private Transform landingPoint;

    [Header("Arc Trajectory")]
    [Tooltip("The height of the arc at its peak, relative to the midpoint between start and end.")]
    [SerializeField] private float arcHeight = 2.0f;

    [Tooltip("The time in seconds the item should take to travel the arc.")]
    [SerializeField] private float travelDuration = 0.7f;

    [Tooltip("The radius around the target position where the item can land.")]
    [SerializeField] private float landingRadius = 1.0f;

    [Header("Gizmo Settings")]
    [Tooltip("The number of points to use for drawing the trajectory Gizmo.")]
    [SerializeField] private int gizmoPathResolution = 20;


    /// <summary>
    /// Spawns the configured item and launches it towards a target position.
    /// </summary>
    /// <param name="itemToSpawn">The item to spawn.</param>
    /// <param name="targetPosition">The central world position where the item should land.</param>
    public void SpawnItem(ItemData itemToSpawn, Vector3 targetPosition = default)
    {
        if (itemToSpawn == null || itemToSpawn.prefab == null)
        {
            Debug.LogError("Cannot spawn item: ItemData or its prefab is not set.", this);
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Cannot spawn item: Spawn Point transform is not set.", this);
            return;
        }

        if(targetPosition == default)
            targetPosition = landingPoint.position;

        // Calculate a random landing spot within the specified radius.
        Vector2 randomOffset = Random.insideUnitCircle * landingRadius;
        Vector3 finalLandingPosition = targetPosition + new Vector3(randomOffset.x, 0, randomOffset.y);

        // Instantiate the item prefab at the starting point.
        GameObject itemObject = Instantiate(itemToSpawn.prefab, spawnPoint.position, Quaternion.identity);

        // Start the coroutine that will move the object along the calculated path.
        StartCoroutine(MoveAlongArcRoutine(itemObject.transform, spawnPoint.position, finalLandingPosition));
    }

    /// <summary>
    /// A coroutine that animates a Transform along a parabolic arc.
    /// </summary>
    private IEnumerator MoveAlongArcRoutine(Transform objectToMove, Vector3 start, Vector3 end)
    {
        //Improvement: let the item hold this logic, just call it here
        TrailRenderer objectTrail = objectToMove.GetComponentInChildren<TrailRenderer>();
        if(objectTrail != null) objectTrail.enabled = true;

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

    /// <summary>
    /// Draws Gizmos in the editor when the object is selected to visualize the spawn trajectory.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Ensure we have a spawn point to draw from.
        if (spawnPoint == null) return;

        // --- Draw the Landing Zone ---
        Handles.color = Color.green;
        // The third parameter is the "normal" or the direction the circle should face. Vector3.up makes it flat on the XZ plane.
        Handles.DrawWireDisc(landingPoint.position, Vector3.up, landingRadius);

        // --- Draw the Arc Path ---
        Gizmos.color = Color.cyan;
        Vector3 previousPoint = spawnPoint.position;

        // Loop through a number of steps to draw the arc.
        for (int i = 1; i <= gizmoPathResolution; i++)
        {
            // Calculate the 't' value (normalized progress) for this step.
            float t = (float)i / gizmoPathResolution;

            // Use the same math as the coroutine to calculate the point on the arc.
            Vector3 linearPosition = Vector3.Lerp(spawnPoint.position, landingPoint.position, t);
            float arc = 4 * arcHeight * (t - (t * t));
            Vector3 currentPoint = linearPosition + new Vector3(0, arc, 0);

            // Draw a line from the previous point to the current one.
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
}

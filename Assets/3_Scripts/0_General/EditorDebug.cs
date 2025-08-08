using UnityEngine;

/// <summary>
/// This component demonstrates the [ExecuteAlways] attribute.
/// The code inside will run in the editor, not just in play mode.
/// This is useful for procedural generation, editor visualizations, or data validation.
/// Note: [ExecuteInEditMode] is the older version of this attribute.
/// </summary>
[ExecuteAlways]
public class EditorDebug : MonoBehaviour
{
    [Header("Gizmo Settings")]
    [Tooltip("The color of the debug sphere.")]
    public Color sphereColor = Color.yellow;

    [Tooltip("The radius of the debug sphere.")]
    [Range(0.1f, 5f)]
    public float sphereRadius = 1f;

    // This method will be called whenever the script is loaded or a value is changed in the Inspector.
    // Because of [ExecuteAlways], it runs outside of play mode.
    void Update()
    {
        // We can check if we are in play mode or in the editor.
        if (Application.isPlaying)
        {
            // Do something only in Play Mode
        }
        else
        {
            // Do something only in the Editor
            // For example, you could update the positions of objects procedurally.
        }

        // This line will print to the console both in and out of play mode.
        // Be careful with this, as it can spam the console.
        // Debug.Log($"EditorDebug Update on object: {gameObject.name}");
    }

    /// <summary>
    /// OnDrawGizmos is called every frame by the editor.
    /// Gizmos are used to give visual debugging or setup aids in the scene view.
    /// This will be drawn regardless of whether the object is selected.
    /// </summary>
    private void OnDrawGizmos()
    {
        // The [ExecuteAlways] attribute ensures that the color and radius values
        // are updated in real-time in the editor.
        Gizmos.color = sphereColor;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}

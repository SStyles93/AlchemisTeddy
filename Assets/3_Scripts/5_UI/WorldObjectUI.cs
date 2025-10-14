using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class WorldObjectUI : MonoBehaviour
{
    [SerializeField] private WorldObject worldObject;
    [SerializeField] private TextMeshPro textMesh;

    private void OnEnable()
    {
        if (worldObject != null)
            worldObject.OnMouseOverUIObject += ShowName;
    }

    private void OnDisable()
    {
        if (worldObject != null)
            worldObject.OnMouseOverUIObject -= ShowName;
    }

    private void Awake()
    {
        if (textMesh == null && TryGetComponent<TextMeshPro>(out var textMeshPro))
            this.textMesh = textMeshPro;

        worldObject ??= GetComponentInParent<WorldObject>();
    }

    private void Start()
    {
        textMesh.enabled = false;
    }

    public void ShowName(string name, bool state)
    {
        textMesh.text = name;
        textMesh.enabled = state;
    }
}

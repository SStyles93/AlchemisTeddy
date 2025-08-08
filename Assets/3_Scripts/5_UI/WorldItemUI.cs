using UnityEngine;
using TMPro;

public class WorldItemUI : MonoBehaviour
{
    [SerializeField] private WorldItem worldItem;
    [SerializeField] private TextMeshPro textMesh;

    private void OnEnable()
    {
        if (worldItem != null)
            worldItem.OnMouseOverObject += ShowName;
    }

    private void OnDisable()
    {
        if (worldItem != null)
            worldItem.OnMouseOverObject -= ShowName;
    }

    private void Awake()
    {
        if (textMesh == null && TryGetComponent<TextMeshPro>(out var textMeshPro))
            this.textMesh = textMeshPro;
        // null-coalescing assignment operator (??)
        // It only executes the right side if textMesh is null.
        worldItem ??= GetComponentInParent<WorldItem>();
    }

    private void Start()
    {
        textMesh.enabled = false;
    }

    private void ShowName(string name, bool state)
    {
        textMesh.text = name;
        textMesh.enabled = state;
    }
}

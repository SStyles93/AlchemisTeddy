using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class WorldObjectUI : MonoBehaviour
{
    [SerializeField] private WorldObject worldObject;
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private MeshRenderer meshRenderer;

    private Material materialInstance;

    private void OnEnable()
    {
        if (worldObject != null)
            worldObject.OnMouseOverObject += ShowUI;
    }

    private void OnDisable()
    {
        if (worldObject != null)
            worldObject.OnMouseOverObject -= ShowUI;
    }

    private void Awake()
    {
        if (textMesh == null && TryGetComponent<TextMeshPro>(out var textMeshPro))
            this.textMesh = textMeshPro;
        if (meshRenderer == null && TryGetComponent<MeshRenderer>(out var renderer))
            this.meshRenderer = renderer;

        worldObject ??= GetComponentInParent<WorldObject>();
    }

    private void Start()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
            materialInstance = new Material(meshRenderer.material);
        }
    }

    public void ShowUI(object uiObject, bool state)
    {
        if (uiObject is string name)
        {
            textMesh.text = name;
        }
        if (uiObject is Texture2D texture)
        {
            if (state == true)
            {
                materialInstance.SetTexture("_BaseMap", texture);
                meshRenderer.material = materialInstance;
            }
        }
        meshRenderer.enabled = state;
    }
}

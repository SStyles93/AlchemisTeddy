using TMPro;
using UnityEngine;

public class WorldObjectImage : MonoBehaviour
{
    [SerializeField] private WorldObject worldObject;
    [SerializeField] private MeshRenderer meshRenderer;

    private Material materialInstance;

    private void OnEnable()
    {
        if (worldObject != null)
            worldObject.OnMouseOver3DObject += ShowImage;
    }

    private void OnDisable()
    {
        if (worldObject != null)
            worldObject.OnMouseOver3DObject -= ShowImage;
    }

    private void Awake()
    {
        if (meshRenderer == null && TryGetComponent<MeshRenderer>(out var renderer))
            this.meshRenderer = renderer;

        worldObject ??= GetComponentInParent<WorldObject>();
    }

    private void Start()
    {
        meshRenderer.enabled = false;
        materialInstance = new Material(meshRenderer.material);
    }

    public void ShowImage(Texture2D texture, bool state)
    {
        if (state == true)
        {
            materialInstance.SetTexture("_BaseMap", texture);
            meshRenderer.material = materialInstance;
        }
        meshRenderer.enabled = state;
    }
}

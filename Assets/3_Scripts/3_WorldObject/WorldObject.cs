using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The Behaviour of WorldObject (IPointer(Enter/Exit)Handler requires PhysicsRaycaster on Camera
/// </summary>
public abstract class WorldObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("WorldObject Attributes")]
    [Tooltip("World Object name\nFacultative: Fallback to Scene name if empty")]
    [SerializeField] protected string objectName;

    public event Action<string, bool> OnMouseOverObject;

    public virtual string DisplayName => objectName != "" ? objectName : this.name;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseOverObject?.Invoke(DisplayName, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnMouseOverObject?.Invoke(DisplayName, false);
    }
}

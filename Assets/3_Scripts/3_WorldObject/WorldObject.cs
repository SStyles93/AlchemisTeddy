using MyBox;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ObjectType
{
    TwoD,
    ThreeD
}

/// <summary>
/// The Behaviour of WorldObject (IPointer(Enter/Exit)Handler requires PhysicsRaycaster on Camera
/// </summary>
public abstract class WorldObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("WorldObject Attributes")]
    [SerializeField] protected ObjectType objectType;

    [ConditionalField(nameof(objectType), false, ObjectType.TwoD)]
    [Tooltip("World Object name\nFacultative: Fallback to Scene name if empty")]
    [SerializeField] protected string objectName;

    [ConditionalField(nameof(objectType), false, ObjectType.ThreeD)]
    [Tooltip("World Object name\nFacultative: Fallback to Scene name if empty")]
    [SerializeField] protected Texture2D objectImage;

    public virtual string DisplayName => objectName != "" ? objectName : this.name;

    public event Action<object, bool> OnMouseOverObject;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseOverObject?.Invoke(objectType == ObjectType.TwoD ? DisplayName : objectImage, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnMouseOverObject?.Invoke(objectType == ObjectType.TwoD ? DisplayName : objectImage, false);
    }

    public ObjectType GetObjectType()
    {
        return objectType;
    }
}

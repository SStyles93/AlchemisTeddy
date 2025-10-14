using MyBox;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The Behaviour of WorldObject (IPointer(Enter/Exit)Handler requires PhysicsRaycaster on Camera
/// </summary>
public abstract class WorldObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected enum ObjectType
    {
        TwoD,
        ThreeD
    }

    [Header("WorldObject Attributes")]
    [SerializeField] protected ObjectType objectType;

    [ConditionalField(nameof(objectType),false, ObjectType.TwoD)]
    [Tooltip("World Object name\nFacultative: Fallback to Scene name if empty")]
    [SerializeField] protected string objectName;

    [ConditionalField(nameof(objectType), false, ObjectType.ThreeD)]
    [Tooltip("World Object name\nFacultative: Fallback to Scene name if empty")]
    [SerializeField] protected Texture2D objectImage;

    public virtual string DisplayName => objectName != "" ? objectName : this.name;
    
    public event Action<string, bool> OnMouseOverUIObject;
    public event Action<Texture2D, bool> OnMouseOver3DObject;

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (objectType)
        {
            case ObjectType.TwoD:
                OnMouseOverUIObject?.Invoke(DisplayName, true);
                break;
            case ObjectType.ThreeD:
                OnMouseOver3DObject?.Invoke(objectImage, true);
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        switch (objectType)
        {
            case ObjectType.TwoD:
                OnMouseOverUIObject?.Invoke(DisplayName, false);
                break;
            case ObjectType.ThreeD:
                OnMouseOver3DObject?.Invoke(objectImage, false);
                break;
        }
    }

    protected ObjectType GetObjectType()
    {
        return objectType;
    }
}

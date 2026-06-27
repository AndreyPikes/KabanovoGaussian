using System;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2f, 0);

    [SerializeField] private InteractiveObject interactiveObject;

    public string ObjectName => interactiveObject.Name;
    public string Description => interactiveObject.Description;

    // Возвращает позицию над объектом в мировых координатах
    public Vector3 GetTooltipWorldPosition()
    {
        return transform.position + uiOffset;
    }

    public InteractiveObject UpdateAndGetInteractiveObject()
    {
        interactiveObject.Position = transform.position;
        interactiveObject.Scale = transform.localScale;
        interactiveObject.Rotation = transform.rotation;

        return interactiveObject;
    }
}

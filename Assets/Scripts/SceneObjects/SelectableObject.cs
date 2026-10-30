using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableObject : MonoBehaviour
{
    private InteractiveObjectDto interactiveObject;

    // Событие, которое сообщает, что этот объект изменил свои параметры
    public static event Action OnObjectChanged;

    public string ObjectName => interactiveObject.Family.ToString();
    public string Description => interactiveObject.Type.ToString();

    public InteractiveObjectDto UpdateAndGetInteractiveObject()
    {
        interactiveObject.Position = transform.position;
        interactiveObject.Scale = transform.localScale;
        interactiveObject.Rotation = transform.rotation.eulerAngles;

        return interactiveObject;
    }

    public void Initialize(InteractiveObjectDto loadedData)
    {
        this.interactiveObject = loadedData;
    }

    public void TriggerChanged()
    {
        OnObjectChanged?.Invoke();
    }
}

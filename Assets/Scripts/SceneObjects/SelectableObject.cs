using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SelectableObject : MonoBehaviour
{
    private InteractiveObjectDto interactiveObject;
    private Action<SelectableObject> OnSelectAction;

    public static event Action OnObjectChanged;
    public static event Action OnDeselect;

    public string ObjectName => interactiveObject.Family.ToString();
    public string Description => interactiveObject.Type.ToString();

    public void Initialize(InteractiveObjectDto loadedData, Action<SelectableObject> onSelectAction)
    {
        this.interactiveObject = loadedData;
        this.OnSelectAction = onSelectAction;
    }

    public InteractiveObjectDto UpdateAndGetInteractiveObject()
    {
        interactiveObject.Position = transform.localPosition;
        interactiveObject.Scale = transform.localScale;
        interactiveObject.Rotation = transform.localRotation.eulerAngles;

        return interactiveObject;
    }

    public void TriggerChanged()
    {
        OnObjectChanged?.Invoke();
    }

    // Публичный метод, который теперь будет вызываться из ObjectPlacementHandler
    public void Select()
    {
        OnSelectAction?.Invoke(this);
    }

    public void Deselect()
    {
        OnDeselect?.Invoke();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SelectableObject : MonoBehaviour
{
    [SerializeField] private List<GameObject> variants;
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
        UpdateModel();
    }

    public void UpdateModel()
    {
        for (int i = 0; i < variants.Count; i++)
        {
            if (i == (int)interactiveObject.Family) variants[i].SetActive(true);
            else variants[i].SetActive(false);
        }

        transform.localScale = interactiveObject.Scale;
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
        UpdateModel();
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

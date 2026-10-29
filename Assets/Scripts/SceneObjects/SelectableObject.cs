using System;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2f, 0);

    [SerializeField] private InteractiveObjectDto interactiveObject;

    public string ObjectName => interactiveObject.Name;
    public string Description => interactiveObject.Description;

    public void Initialize(InteractiveObjectDto loadedData)
    {
        // √лубокое или поверхностное копирование данных в зависимоcти от логики.
        // “ак как это класс, просто присваиваем ссылку:
        this.interactiveObject = loadedData;
    }

    public InteractiveObjectDto UpdateAndGetInteractiveObject()
    {
        interactiveObject.Position = transform.position;
        interactiveObject.Scale = transform.localScale;
        interactiveObject.Rotation = transform.rotation;

        return interactiveObject;
    }
}

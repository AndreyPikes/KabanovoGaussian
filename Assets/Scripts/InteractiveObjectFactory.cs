using System;
using UnityEngine;

public interface IInteractiveObjectFactory
{
    // Добавлен параметр Action<SelectableObject> onSelectAction
    SelectableObject Create(InteractiveObjectDto dto, Transform parent, Action<SelectableObject> onSelectAction);
}
public class InteractiveObjectFactory : IInteractiveObjectFactory
{
    public SelectableObject Create(InteractiveObjectDto dto, Transform parent, Action<SelectableObject> onSelectAction)
    {
        string prefabName = dto.Family.ToString();
        SelectableObject prefab = Resources.Load<SelectableObject>($"Prefabs/Не_задано");

        if (prefab == null)
        {
            Debug.LogError($"[Factory] Не удалось найти префаб по пути: Resources/Prefabs/{prefabName}");
            return null;
        }

        SelectableObject instance = UnityEngine.Object.Instantiate(prefab, parent);

        instance.transform.localPosition = dto.Position;
        instance.transform.localRotation = Quaternion.Euler(dto.Rotation);
        instance.transform.localScale = dto.Scale;

        // Передаем и DTO, и колбэк для клика
        instance.Initialize(dto, onSelectAction);

        return instance;
    }
}

using UnityEngine;

public interface IInteractiveObjectFactory
{
    SelectableObject Create(InteractiveObjectDto dto, Transform parent);
}


public class InteractiveObjectFactory : IInteractiveObjectFactory
{
    public SelectableObject Create(InteractiveObjectDto dto, Transform parent)
    {
        string prefabName = dto.Family.ToString();
        SelectableObject prefab = Resources.Load<SelectableObject>($"Prefabs/{prefabName}");

        if (prefab == null)
        {
            Debug.LogError($"[Factory] Не удалось найти префаб по пути: Resources/Prefabs/{prefabName}");
            return null;
        }
        
        SelectableObject instance = Object.Instantiate(prefab, dto.Position, Quaternion.Euler(dto.Rotation), parent);
        instance.transform.localScale = dto.Scale;
        instance.Initialize(dto);

        return instance;
    }
}
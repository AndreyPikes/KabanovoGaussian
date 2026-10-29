using UnityEngine;

public interface IInteractiveObjectFactory
{
    SelectableObject Create(InteractiveObjectDto dto, UnityEngine.Transform parent);
}


public class InteractiveObjectFactory : IInteractiveObjectFactory
{
    public SelectableObject Create(InteractiveObjectDto dto, Transform parent)
    {
        // Превращаем enum в строку, чтобы найти префаб по имени (например, "Tree", "Bush")
        string prefabName = dto.ObjectType.ToString();

        // Загружаем префаб из папки Resources/Prefabs/
        SelectableObject prefab = Resources.Load<SelectableObject>($"Prefabs/{prefabName}");

        if (prefab == null)
        {
            Debug.LogError($"[Factory] Не удалось найти префаб по пути: Resources/Prefabs/{prefabName}");
            return null;
        }

        // Так как фабрика не MonoBehaviour, используем Object.Instantiate
        SelectableObject instance = Object.Instantiate(prefab, dto.Position, dto.Rotation, parent);
        instance.transform.localScale = dto.Scale;
        instance.Initialize(dto);

        return instance;
    }
}
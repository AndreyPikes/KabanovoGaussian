using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneObjectManager : MonoBehaviour
{
    [SerializeField] private Button saveButton;
    [SerializeField] private Transform objectsContainer;

    private IDataManager dataManager;
    private IInteractiveObjectFactory objectFactory;

    private void Awake()
    {
        dataManager = new DataManager();
        objectFactory = new InteractiveObjectFactory();
    }

    private void OnEnable()
    {
        // Подписываемся на событие изменения ЛЮБОГО объекта
        SelectableObject.OnObjectChanged += AutoSave;
    }

    private void OnDisable()
    {
        SelectableObject.OnObjectChanged -= AutoSave;
    }

    private void Start()
    {
        OnLoadPressed();
    }

    private void AutoSave()
    {        
        dataManager.Save(GatherObjectsData());
        Debug.Log("[SceneObjectManager] Автосохранение выполнено успешно.");
    }

    private void OnLoadPressed()
    {
        List<InteractiveObjectDto> loadedData = dataManager.Load();

        if (loadedData == null || loadedData.Count == 0)
        {
            Debug.LogWarning("[SceneObjectManager] Нет данных для загрузки.");
            return;
        }

        ClearContainer();

        foreach (var dto in loadedData)
        {
            objectFactory.Create(dto, objectsContainer);
        }

        Debug.Log($"[SceneObjectManager] Успешно спавнуто объектов: {loadedData.Count}");
    }

    private List<InteractiveObjectDto> GatherObjectsData()
    {
        var containerChildren = objectsContainer.GetComponentsInChildren<SelectableObject>();
        List<InteractiveObjectDto> currentData = new();

        foreach (var obj in containerChildren)
        {
            currentData.Add(obj.UpdateAndGetInteractiveObject());
        }

        return currentData;
    }

    private void ClearContainer()
    {
        foreach (Transform child in objectsContainer)
        {
            Destroy(child.gameObject);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneObjectManager : MonoBehaviour
{
    [SerializeField] private Transform objectsContainer;

    // Событие, которое сообщает UI (и кому угодно), что объект был выбран
    public event Action<SelectableObject> OnObjectSelected;

    private IDataManager dataManager;
    private IInteractiveObjectFactory objectFactory;
    private SelectableObject selectedObject; // Текущий выделенный объект

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

    // Метод для установки текущего активного объекта
    private void SelectObject(SelectableObject obj)
    {
        selectedObject = obj;
        OnObjectSelected?.Invoke(selectedObject);
    }

    private void AutoSave()
    {        
        dataManager.Save(GatherObjectsData());
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
            // Передаем метод SelectObject как делегат прямо в фабрику
            objectFactory.Create(dto, objectsContainer, SelectObject);
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
        selectedObject = null;
        OnObjectSelected?.Invoke(null); // Оповещаем UI, что выбор сброшен

        foreach (Transform child in objectsContainer)
        {
            Destroy(child.gameObject);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneObjectManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button saveButton;

    [Header("Scene Container")]
    [SerializeField] private Transform objectsContainer;

    // Ссылки на интерфейсы (создаются внутри Awake)
    private IDataManager _dataManager;
    private IInteractiveObjectFactory _objectFactory;

    private void Awake()
    {
        // Инициализируем чистые C# классы
        _dataManager = new DataManager();
        _objectFactory = new InteractiveObjectFactory();
    }

    private void Start()
    {
        // Обрабатываем нажатия кнопок прямо здесь
        saveButton.onClick.AddListener(OnSavePressed);
        OnLoadPressed();
    }

    private void OnSavePressed()
    {
        List<InteractiveObjectDto> dataToSave = GatherObjectsData();
        _dataManager.Save(dataToSave);
    }

    private void OnLoadPressed()
    {
        List<InteractiveObjectDto> loadedData = _dataManager.Load();

        if (loadedData == null || loadedData.Count == 0)
        {
            Debug.LogWarning("[SceneObjectManager] Нет данных для загрузки.");
            return;
        }

        ClearContainer();

        foreach (var dto in loadedData)
        {
            _objectFactory.Create(dto, objectsContainer);
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

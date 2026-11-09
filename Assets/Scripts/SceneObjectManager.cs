using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        SelectableObject.OnDeselect += DeselectObject;
    }

    private void OnDisable()
    {
        SelectableObject.OnObjectChanged -= AutoSave;
        SelectableObject.OnDeselect -= DeselectObject;
    }

    private void Start()
    {
        OnLoadPressed();
    }

    private void DeselectObject()
    {
        SelectObject(null);
    }

    // Метод для установки текущего активного объекта
    private void SelectObject(SelectableObject obj)
    {
        selectedObject = obj;
        OnObjectSelected?.Invoke(selectedObject);
    }

    public void AddNewObject()
    {
        // Получаем главную камеру сцены
        Camera mainCamera = Camera.main;
        Vector3 localSpawnPosition = Vector3.zero;

        if (mainCamera != null)
        {
            // Создаем луч из центра экрана камеры вперед
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            // Пытаемся найти точку пересечения с поверхностью (например, с землей)
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {                
                localSpawnPosition = objectsContainer.InverseTransformPoint(hit.point);
                Debug.LogError(localSpawnPosition.ToString());
            }            
        }
        else
        {
            Debug.LogWarning("[SceneObjectManager] Главная камера (Camera.main) не найдена! Объект заспавнен в (0,0,0).");
        }


        InteractiveObjectDto newDto = new InteractiveObjectDto
        {
            Position = new Vector3(localSpawnPosition.x, 0f, localSpawnPosition.z), // Или выберите позицию перед камерой / по центру
            Scale = Vector3.one,
            Rotation = Vector3.zero,
            Type = ObjectType.Не_задано,
            Family = PlantFamily.Не_задано,
            Ripening = RipeningPeriod.Не_задано,
            Sort = "",
            PlantingYear = 0,
            Features = ""
        };

        SelectableObject newObj = objectFactory.Create(newDto, objectsContainer, SelectObject);
        if (newObj != null)
        {
            SelectObject(newObj);
            AutoSave();
        }
    }

    public async void DeleteSelectedObject()
    {
        if (selectedObject == null) return;

        Destroy(selectedObject.gameObject);
        SelectObject(null);

        await Task.Delay(100);

        AutoSave();
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

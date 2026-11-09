using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SceneObjectUiController : MonoBehaviour
{
    [SerializeField] private Button AddObject;
    [SerializeField] private Button DeleteObject;
    [Header("UI Components")]
    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown familyDropdown;
    [SerializeField] private TMP_Dropdown ripeningDropdown;
    [SerializeField] private TMP_InputField sortInputField;
    [SerializeField] private TMP_InputField yearInputField;
    [SerializeField] private TMP_InputField featuresInputField;
    [SerializeField] private Button smaller;
    [SerializeField] private Button bigger;

    private SceneObjectManager objectManager;
    private InteractiveObjectDto currentDto;
    private SelectableObject currentObject;

    private void Awake()
    {
        objectManager = FindAnyObjectByType<SceneObjectManager>();
        InitializeDropdowns();

        // Скрываем UI по умолчанию в самом старте
        ToggleUiVisibility(false);
    }

    private void OnEnable()
    {
        typeDropdown.onValueChanged.AddListener(OnTypeChanged);
        familyDropdown.onValueChanged.AddListener(OnFamilyChanged);
        ripeningDropdown.onValueChanged.AddListener(OnRipeningChanged);
        sortInputField.onEndEdit.AddListener(OnSortChanged);
        yearInputField.onEndEdit.AddListener(OnYearChanged);
        featuresInputField.onEndEdit.AddListener(OnFeaturesChanged);

        if (objectManager != null)
        {
            objectManager.OnObjectSelected += ShowObjectData;
        }

        if (AddObject != null) AddObject.onClick.AddListener(OnAddObjectPressed);
        if (DeleteObject != null) DeleteObject.onClick.AddListener(OnDeleteObjectPressed);
        smaller.onClick.AddListener(SetSmaller);
        bigger.onClick.AddListener(SetBigger);
    }

    private void OnDisable()
    {
        typeDropdown.onValueChanged.RemoveListener(OnTypeChanged);
        familyDropdown.onValueChanged.RemoveListener(OnFamilyChanged);
        ripeningDropdown.onValueChanged.RemoveListener(OnRipeningChanged);
        sortInputField.onEndEdit.RemoveListener(OnSortChanged);
        yearInputField.onEndEdit.RemoveListener(OnYearChanged);
        featuresInputField.onEndEdit.RemoveListener(OnFeaturesChanged);

        if (objectManager != null)
        {
            objectManager.OnObjectSelected -= ShowObjectData;
        }

        if (AddObject != null) AddObject.onClick.RemoveListener(OnAddObjectPressed);
        if (DeleteObject != null) DeleteObject.onClick.RemoveListener(OnDeleteObjectPressed);
        smaller.onClick.RemoveListener(SetSmaller);
        bigger.onClick.RemoveListener(SetBigger);
    }

    private void ShowObjectData(SelectableObject selectedObject)
    {
        currentObject = selectedObject;

        // Если ничего не выбрано (или выбор сброшен/объект удален) — скрываем UI
        if (currentObject == null)
        {
            ToggleUiVisibility(false);
            return;
        }

        currentDto = currentObject.UpdateAndGetInteractiveObject();

        typeDropdown.SetValueWithoutNotify((int)currentDto.Type);
        familyDropdown.SetValueWithoutNotify((int)currentDto.Family);
        ripeningDropdown.SetValueWithoutNotify((int)currentDto.Ripening);
        sortInputField.SetTextWithoutNotify(currentDto.Sort);
        yearInputField.SetTextWithoutNotify(currentDto.PlantingYear.ToString());
        featuresInputField.SetTextWithoutNotify(currentDto.Features);

        // Показываем UI, так как объект успешно выбран и данные заполнены
        ToggleUiVisibility(true);
    }

    private void OnAddObjectPressed()
    {
        if (objectManager != null)
        {
            objectManager.AddNewObject();
        }
    }

    private void OnDeleteObjectPressed()
    {
        if (objectManager != null)
        {
            objectManager.DeleteSelectedObject();
        }
    }

    private void OnTypeChanged(int index)
    {
        if (currentObject == null || currentDto == null) return;
        currentDto.Type = (ObjectType)index;
        currentObject.TriggerChanged();
    }

    private void OnFamilyChanged(int index)
    {
        if (currentObject == null || currentDto == null) return;
        currentDto.Family = (PlantFamily)index;
        currentObject.TriggerChanged();
    }

    private void OnRipeningChanged(int index)
    {
        if (currentObject == null || currentDto == null) return;
        currentDto.Ripening = (RipeningPeriod)index;
        currentObject.TriggerChanged();
    }

    private void OnSortChanged(string value)
    {
        if (currentObject == null || currentDto == null) return;
        currentDto.Sort = value;
        currentObject.TriggerChanged();
    }

    private void OnYearChanged(string value)
    {
        if (currentObject == null || currentDto == null) return;
        if (int.TryParse(value, out int year))
        {
            currentDto.PlantingYear = year;
            currentObject.TriggerChanged();
        }
    }

    private void OnFeaturesChanged(string value)
    {
        if (currentObject == null || currentDto == null) return;
        currentDto.Features = value;
        currentObject.TriggerChanged();
    }

    private void SetSmaller()
    {
        currentDto.Scale -= Vector3.one * 0.1f;
        currentObject.TriggerChanged();
    }

    private void SetBigger()
    {
        currentDto.Scale += Vector3.one * 0.1f;
        currentObject.TriggerChanged();
    }

    // Вспомогательный метод для управления видимостью и кликабельностью CanvasGroup
    private void ToggleUiVisibility(bool isVisible)
    {
        if (canvas == null) return;

        canvas.alpha = isVisible ? 1f : 0f;               // Делаем видимым или прозрачным
        canvas.interactable = isVisible;                 // Включаем/выключаем взаимодействие с кнопками/полями
        canvas.blocksRaycasts = isVisible;               // Разрешаем или запрещаем UI перехватывать клики мыши
    }

    private void InitializeDropdowns()
    {
        PopulateDropdownWithEnum<ObjectType>(typeDropdown);
        PopulateDropdownWithEnum<PlantFamily>(familyDropdown);
        PopulateDropdownWithEnum<RipeningPeriod>(ripeningDropdown);
    }

    private void PopulateDropdownWithEnum<T>(TMP_Dropdown dropdown) where T : System.Enum
    {
        dropdown.ClearOptions();
        List<string> options = new List<string>(System.Enum.GetNames(typeof(T)));
        List<string> optionsWithSpaces = new List<string>(options.Count);
        foreach (var option in options) 
        {            
            optionsWithSpaces.Add(option.Replace("_", " "));
        }
        dropdown.AddOptions(optionsWithSpaces);
    }
}

using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SceneObjectUiController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown familyDropdown;
    [SerializeField] private TMP_Dropdown ripeningDropdown;
    [SerializeField] private TMP_InputField yearInputField;
    [SerializeField] private TMP_InputField featuresInputField;

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
        yearInputField.onEndEdit.AddListener(OnYearChanged);
        featuresInputField.onEndEdit.AddListener(OnFeaturesChanged);

        if (objectManager != null)
        {
            objectManager.OnObjectSelected += ShowObjectData;
        }
    }

    private void OnDisable()
    {
        typeDropdown.onValueChanged.RemoveListener(OnTypeChanged);
        familyDropdown.onValueChanged.RemoveListener(OnFamilyChanged);
        ripeningDropdown.onValueChanged.RemoveListener(OnRipeningChanged);
        yearInputField.onEndEdit.RemoveListener(OnYearChanged);
        featuresInputField.onEndEdit.RemoveListener(OnFeaturesChanged);

        if (objectManager != null)
        {
            objectManager.OnObjectSelected -= ShowObjectData;
        }
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
        yearInputField.SetTextWithoutNotify(currentDto.PlantingYear.ToString());
        featuresInputField.SetTextWithoutNotify(currentDto.Features);

        // Показываем UI, так как объект успешно выбран и данные заполнены
        ToggleUiVisibility(true);
    }

    #region UI Listeners

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

    #endregion

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
        dropdown.AddOptions(options);
    }
}

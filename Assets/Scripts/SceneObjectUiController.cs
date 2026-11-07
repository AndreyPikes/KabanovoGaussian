using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SceneObjectUiController : MonoBehaviour
{
    [Header("UI Components")]
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
    }

    private void OnEnable()
    {
        // Подписываемся на изменения в UI
        typeDropdown.onValueChanged.AddListener(OnTypeChanged);
        familyDropdown.onValueChanged.AddListener(OnFamilyChanged);
        ripeningDropdown.onValueChanged.AddListener(OnRipeningChanged);
        yearInputField.onEndEdit.AddListener(OnYearChanged);
        featuresInputField.onEndEdit.AddListener(OnFeaturesChanged);

        // Подписываемся на событие выделения объекта в менеджере
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

    // Метод вызывается, когда менеджер сообщает о выборе нового объекта
    private void ShowObjectData(SelectableObject selectedObject)
    {
        currentObject = selectedObject;

        if (currentObject == null)
        {
            // Если ничего не выбрано, можно заблокировать или очистить UI
            return;
        }

        // Получаем актуальный DTO из объекта
        currentDto = currentObject.UpdateAndGetInteractiveObject();

        // Заполняем UI без вызова событий (чтобы не зациклить логику)
        typeDropdown.SetValueWithoutNotify((int)currentDto.Type);
        familyDropdown.SetValueWithoutNotify((int)currentDto.Family);
        ripeningDropdown.SetValueWithoutNotify((int)currentDto.Ripening);
        yearInputField.SetTextWithoutNotify(currentDto.PlantingYear.ToString());
        featuresInputField.SetTextWithoutNotify(currentDto.Features);
    }

    #region UI Listeners

    private void OnTypeChanged(int index)
    {
        if (currentObject == null || currentDto == null) return;
        currentDto.Type = (ObjectType)index;
        currentObject.TriggerChanged(); // Сообщаем системе об изменениях для автосохранения
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

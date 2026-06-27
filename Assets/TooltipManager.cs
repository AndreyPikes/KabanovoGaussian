using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask interactableLayer;

    private Camera mainCamera;
    private SelectableObject currentTarget;

    private void Start()
    {
        mainCamera = Camera.main;
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        // 1. Получаем позицию мыши/тача через New Input System
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // 2. Пускаем луч из камеры в сцену
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableLayer))
        {
            // Проверяем, есть ли на объекте наш скрипт
            SelectableObject selectable = hit.collider.GetComponent<SelectableObject>();

            if (selectable != null)
            {
                if (currentTarget != selectable)
                {
                    currentTarget = selectable;
                    ShowTooltip(selectable);
                }
                
                // Обновляем позицию UI каждый кадр, пока мышь над объектом
                // Было: UpdateTooltipPosition(selectable, mousePosition);
                // Стало: передаем точку попадания луча hit.point
                UpdateTooltipPosition(hit.point); 
                return;
            }
        }

        // Если луч никуда не попал или попал на объект без скрипта
        if (currentTarget != null)
        {
            HideTooltip();
        }
    }

    private void ShowTooltip(SelectableObject obj)
    {
        if (tooltipPanel == null) return;

        if (nameText != null) nameText.text = obj.ObjectName;
        if (descriptionText != null) descriptionText.text = obj.Description;

        tooltipPanel.SetActive(true);
    }

    private void UpdateTooltipPosition(Vector3 hitPoint)
{
    if (tooltipPanel == null) return;

    // Конвертируем точную точку на поверхности в экранные координаты
    Vector3 screenPos = mainCamera.WorldToScreenPoint(hitPoint);

    if (screenPos.z < 0)
    {
        tooltipPanel.SetActive(false);
        return;
    }

    // Привязываем UI к этой точке
    tooltipPanel.transform.position = new Vector3(screenPos.x, screenPos.y, 0);
}


    private void HideTooltip()
    {
        currentTarget = null;
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }
}

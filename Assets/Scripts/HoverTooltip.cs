using UnityEngine;

public class HoverTooltip : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject tooltipPanel; // Ссылка на UI панель canvas'а
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0); // Смещение UI над объектом

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false); // Прячем окно на старте
        }
    }

    private void OnMouseEnter()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            UpdateTooltipPosition();
        }
    }

    private void OnMouseOver()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            UpdateTooltipPosition();
        }
    }

    private void OnMouseExit()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void UpdateTooltipPosition()
    {
        // Конвертируем мировые координаты объекта (с учетом смещения) в экранные координаты для UI
        Vector3 worldPosition = transform.position + offset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        
        // Устанавливаем позицию UI панели
        tooltipPanel.transform.position = screenPosition;
    }
}

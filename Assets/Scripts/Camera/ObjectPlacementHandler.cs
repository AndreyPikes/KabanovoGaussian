using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPlacementHandler
{
    private readonly Camera _cam;
    private readonly LayerMask _interactableLayer;
    private readonly LayerMask _groundLayer;

    private SelectableObject draggedObject;
    private Plane _dragPlane;
    private Vector3 _cursorOffset;

    public bool IsDraggingObject => draggedObject != null;

    public ObjectPlacementHandler(Camera cam, LayerMask interactableLayer, LayerMask groundLayer)
    {
        _cam = cam;
        _interactableLayer = interactableLayer;
        _groundLayer = groundLayer;
    }

    // Изменяем метод: теперь он обрабатывает ЛЮБОЙ клик по объекту
    public bool TryProcessClick(Vector2 mousePosition)
    {
        Ray ray = _cam.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _interactableLayer))
        {
            SelectableObject selectable = hit.collider.GetComponent<SelectableObject>();
            if (selectable != null)
            {
                // СЦЕНАРИЙ 1: Зажат Ctrl -> Начинаем перетаскивание
                if (Keyboard.current.ctrlKey.isPressed)
                {
                    draggedObject = selectable;
                    _dragPlane = new Plane(Vector3.up, draggedObject.transform.position);

                    if (_dragPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        _cursorOffset = draggedObject.transform.position - hitPoint;
                    }
                    return true; // Возвращаем true, так как начался Drag
                }
                // СЦЕНАРИЙ 2: Ctrl НЕ зажат -> Это обычный клик для выделения
                else
                {
                    selectable.Select(); // Мгновенно выделяет объект и обновляет UI
                    return false; // Drag не начался
                }
            }
        }
        return false;
    }

    // Обновляем позицию объекта каждый кадр (остается без изменений)
    public void DragObject(Vector2 mousePosition)
    {
        if (draggedObject == null) return;

        Ray ray = _cam.ScreenPointToRay(mousePosition);
        if (_dragPlane.Raycast(ray, out float enter))
        {
            Vector3 targetPoint = ray.GetPoint(enter);
            Vector3 newPosition = targetPoint + _cursorOffset;

            newPosition.y = draggedObject.transform.position.y;
            draggedObject.transform.position = newPosition;

            draggedObject.UpdateAndGetInteractiveObject();
        }
    }

    public void StopDrag()
    {
        if (draggedObject != null)
        {
            draggedObject.TriggerChanged();
        }

        draggedObject = null;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPlacementHandler
{
    private readonly Camera _cam;
    private readonly LayerMask _interactableLayer;
    private readonly LayerMask _groundLayer;

    private SelectableObject _draggedObject;
    private Plane _dragPlane;
    private Vector3 _cursorOffset;

    public bool IsDraggingObject => _draggedObject != null;

    public ObjectPlacementHandler(Camera cam, LayerMask interactableLayer, LayerMask groundLayer)
    {
        _cam = cam;
        _interactableLayer = interactableLayer;
        _groundLayer = groundLayer;
    }

    // Проверяем, зажали ли мы Ctrl и попали ли по объекту
    public bool TryStartDrag(Vector2 mousePosition)
    {
        // Проверяем нажатие клавиатурного Ctrl через New Input System
        if (!Keyboard.current.ctrlKey.isPressed) return false;

        Ray ray = _cam.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _interactableLayer))
        {
            SelectableObject selectable = hit.collider.GetComponent<SelectableObject>();
            if (selectable != null)
            {
                _draggedObject = selectable;

                // Создаем математическую плоскость на высоте объекта, параллельно земле (Y)
                _dragPlane = new Plane(Vector3.up, _draggedObject.transform.position);

                // Находим точку пересечения луча с этой плоскостью для расчета смещения курсора
                if (_dragPlane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    // Запоминаем, в какое именно место объекта мы кликнули, чтобы он не прыгал центром в курсор
                    _cursorOffset = _draggedObject.transform.position - hitPoint;
                }
                return true;
            }
        }
        return false;
    }

    // Обновляем позицию объекта каждый кадр
    public void DragObject(Vector2 mousePosition)
    {
        if (_draggedObject == null) return;

        Ray ray = _cam.ScreenPointToRay(mousePosition);
        if (_dragPlane.Raycast(ray, out float enter))
        {
            Vector3 targetPoint = ray.GetPoint(enter);
            Vector3 newPosition = targetPoint + _cursorOffset;

            // Фиксируем координату Y, меняем только X и Z
            newPosition.y = _draggedObject.transform.position.y;

            _draggedObject.transform.position = newPosition;

            // Принудительно обновляем внутренние данные InteractiveObject для последующего сохранения
            _draggedObject.UpdateAndGetInteractiveObject();
        }
    }

    // Сбрасываем перетаскивание
    public void StopDrag()
    {
        _draggedObject = null;
    }
}

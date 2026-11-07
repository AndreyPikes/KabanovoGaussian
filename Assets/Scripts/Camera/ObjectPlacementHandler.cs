using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPlacementHandler
{
    private readonly Camera cam;
    private readonly LayerMask interactableLayer;
    private readonly LayerMask groundLayer;

    private SelectableObject draggedObject;
    private Plane dragPlane;
    private Vector3 _cursorOffset;
    private SelectableObject lastSelectedObject;

    public bool IsDraggingObject => draggedObject != null;

    // Добавляем SceneObjectManager в конструктор
    public ObjectPlacementHandler(Camera cam, LayerMask interactableLayer, LayerMask groundLayer)
    {
        this.cam = cam;
        this.interactableLayer = interactableLayer;
        this.groundLayer = groundLayer;
    }

    public bool TryProcessClick(Vector2 mousePosition)
    {
        Ray ray = cam.ScreenPointToRay(mousePosition);

        // 1. Проверяем попадание по интерактивному объекту
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            SelectableObject selectable = hit.collider.GetComponent<SelectableObject>();
            if (selectable != null)
            {
                lastSelectedObject = selectable;
                // Режим Drag (с зажатым Ctrl)
                if (Keyboard.current.ctrlKey.isPressed)
                {
                    draggedObject = selectable;
                    dragPlane = new Plane(Vector3.up, draggedObject.transform.position);

                    if (dragPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        _cursorOffset = draggedObject.transform.position - hitPoint;
                    }
                    return true;
                }
                // Режим Выделения (обычный клик)
                else
                {
                    selectable.Select();
                    return false;
                }
            }
        }

        // 2. ЕСЛИ МИМО: Если мы попали в землю ИЛИ вообще никуда не попали (промах в небо)
        // При этом Ctrl не должен быть зажат (чтобы случайно не сбрасывать при перетаскивании)
        if (!Keyboard.current.ctrlKey.isPressed)
        {
            if (lastSelectedObject != null)
            {
                lastSelectedObject.Deselect(); // Вызываем метод снятия выделения
            }
        }

        return false;
    }

    public void DragObject(Vector2 mousePosition)
    {
        if (draggedObject == null) return;

        Ray ray = cam.ScreenPointToRay(mousePosition);
        if (dragPlane.Raycast(ray, out float enter))
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

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class MapCameraController : MonoBehaviour
{
    [Header("Настройки объектов")]
    [SerializeField] private LayerMask interactableLayer; // Слой, на котором висят SelectableObject

    private ObjectPlacementHandler objectPlacementModule;

    [Header("Настройки ввода")]
    [SerializeField] private InputAction mouseClickAction;
    [SerializeField] private InputAction mousePositionAction;
    [SerializeField] private InputAction mouseScrollAction;
    [SerializeField] private InputAction rotateClickAction;
    [SerializeField] private InputAction mouseDeltaAction;

    [Header("Режим отображения")]
    [SerializeField] private bool isOrthographicMode = false;
    [SerializeField] private float orthoZoomSpeed = 5f;

    [Header("Настройки движения")]
    [SerializeField] private float smoothness = 10f;

    [Header("Настройки поворота")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float rotationSmoothness = 5f;

    [Header("Настройки зума")]
    [SerializeField] private float zoomSpeed = 15f;
    [SerializeField] private float zoomSmoothness = 10f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 50f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ограничения по осям X/Z")]
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float targetRotationY;

    private bool isDragging;
    private bool isRotating;
    private Camera cam;
    private bool lastFrameOrthoMode;
    private bool skipFirstDragFrame;

    // Ссылки на подмодули логики
    private CameraMovement movementModule;
    private CameraRotation rotationModule;
    private CameraZoom zoomModule;

    private void OnEnable()
    {
        ToggleActions(true);
        mouseClickAction.started += OnDragStarted;
        mouseClickAction.canceled += OnDragCanceled;
        rotateClickAction.started += OnRotationStarted;
        rotateClickAction.canceled += OnRotationCanceled;
    }

    private void OnDisable()
    {
        mouseClickAction.started -= OnDragStarted;
        mouseClickAction.canceled -= OnDragCanceled;
        rotateClickAction.started -= OnRotationStarted;
        rotateClickAction.canceled -= OnRotationCanceled;
        ToggleActions(false);
    }

    private void ToggleActions(bool enable)
    {
        if (enable) { mouseClickAction.Enable(); mousePositionAction.Enable(); mouseScrollAction.Enable(); rotateClickAction.Enable(); mouseDeltaAction.Enable(); }
        else { mouseClickAction.Disable(); mousePositionAction.Disable(); mouseScrollAction.Disable(); rotateClickAction.Disable(); mouseDeltaAction.Disable(); }
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        targetPosition = transform.position;
        targetRotationY = transform.rotation.eulerAngles.y;

        movementModule = new CameraMovement(cam, groundLayer);
        rotationModule = new CameraRotation(cam, groundLayer);
        zoomModule = new CameraZoom(cam, targetPosition.y);

        // ИНИЦИАЛИЗАЦИЯ: Новый модуль для объектов
        objectPlacementModule = new ObjectPlacementHandler(cam, interactableLayer, groundLayer);

        lastFrameOrthoMode = isOrthographicMode;
        ApplyCameraProjection(isOrthographicMode);

        targetRotation = Quaternion.Euler(isOrthographicMode ? 90f : 45f, targetRotationY, 0f);
        transform.rotation = targetRotation;
    }


    void Update()
    {
        if (isOrthographicMode != lastFrameOrthoMode) SetOrthographicMode(isOrthographicMode);

        HandleZoom();
        HandleRotation();

        // Если мы тащим объект, обрабатываем его перемещение
        if (objectPlacementModule.IsDraggingObject)
        {
            objectPlacementModule.DragObject(mousePositionAction.ReadValue<Vector2>());
        }
        else
        {
            // Иначе двигаем саму камеру
            HandleMouseDrag();
        }

        SmoothMove();
    }

    private void OnDragStarted(InputAction.CallbackContext context)
    {
        if (isRotating) return;

        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();

        // 1. Проверяем, пытаемся ли мы перетащить объект с зажатым Ctrl
        if (objectPlacementModule.TryStartDrag(mousePos))
        {
            isDragging = false; // Отключаем drag камеры
            return;
        }

        // 2. Если Ctrl не зажат, двигаем саму камеру (старый рабочий код)
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        targetRotationY = transform.rotation.eulerAngles.y;

        skipFirstDragFrame = true;
        isDragging = true;
    }

    private void OnDragCanceled(InputAction.CallbackContext context)
    {
        isDragging = false;
        objectPlacementModule.StopDrag(); // Всегда сбрасываем перенос объекта при отпускании мыши
    }

    private void OnRotationStarted(InputAction.CallbackContext context)
    {
        isRotating = true;
        isDragging = false;
        rotationModule.LockPivot();
    }

    private void OnRotationCanceled(InputAction.CallbackContext context) => isRotating = false;

    private void HandleMouseDrag()
    {
        if (!isDragging || isRotating) return;

        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();

        // ИСПРАВЛЕНИЕ: Защита от потери фокуса. 
        // В самый первый кадр после клика мы не двигаем камеру, а лишь фиксируем 
        // ТОЧНЫЕ и уже обновленные координаты мыши на земле.
        if (skipFirstDragFrame)
        {
            movementModule.StartDrag(mousePos);
            skipFirstDragFrame = false;
            return; // Пропускаем расчет дельты в этом кадре
        }

        // Во всех последующих кадрах drag работает в штатном режиме
        Vector3 delta = movementModule.CalculateDragDelta(mousePos, targetPosition, targetRotation);

        targetPosition.x += delta.x;
        targetPosition.z += delta.z;
        ClampPosition();
    }


    private void HandleRotation()
    {
        if (!isRotating) return;

        float mouseDeltaX = mouseDeltaAction.ReadValue<Vector2>().x;
        rotationModule.CalculateRotation(mouseDeltaX, rotationSpeed, ref targetRotationY, ref targetRotation, ref targetPosition, isOrthographicMode);
        ClampPosition();
    }

    private void HandleZoom()
    {
        float scrollY = mouseScrollAction.ReadValue<Vector2>().y;
        zoomModule.AddZoomInput(scrollY, isOrthographicMode, zoomSpeed, orthoZoomSpeed, minHeight, maxHeight);
        zoomModule.ExecuteZoom(ref targetPosition, targetRotation, isOrthographicMode, zoomSmoothness, minHeight, maxHeight);

        if (!isOrthographicMode) ClampPosition();
    }

    private void ClampPosition()
    {
        targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
    }

    private void SmoothMove()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothness);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothness);
    }

    public void SetOrthographicMode(bool enable)
{
    isOrthographicMode = enable;
    lastFrameOrthoMode = isOrthographicMode;
    ApplyCameraProjection(isOrthographicMode);
    targetRotation = Quaternion.Euler(isOrthographicMode ? 90f : 45f, targetRotationY, 0f);

   
}


    private void ApplyCameraProjection(bool ortho)
    {
        if (cam == null) cam = GetComponent<Camera>();
        cam.orthographic = ortho;

        if (ortho)
        {
            zoomModule.SetTargets(targetPosition.y);
            cam.orthographicSize = targetPosition.y;
        }
        else
        {
            zoomModule.SetTargets(cam.orthographicSize);
            targetPosition.y = cam.orthographicSize;
        }
    }
}

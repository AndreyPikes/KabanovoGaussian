using UnityEngine;
using UnityEngine.InputSystem;

public class MapCameraController : MonoBehaviour
{
    [Header("Настройки ввода")]
    [SerializeField] private InputAction mouseClickAction;
    [SerializeField] private InputAction mousePositionAction;
    [SerializeField] private InputAction mouseScrollAction;
    [SerializeField] private InputAction rotateClickAction; // Кнопка для зажатия поворота (например, MMB или RMB)
    [SerializeField] private InputAction mouseDeltaAction;  // Смещение мыши (Value -> Vector2)

    [Header("Настройки движения")]
    [SerializeField] private float smoothness = 10f;

    [Header("Настройки поворота")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private float rotationSmoothness = 5f;

    [Header("Настройки зума")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 50f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ограничения по осям X/Z")]
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Vector3 targetPosition;
    private Quaternion targetRotation; // Целевой поворот для сглаживания
    private float targetRotationY;     // Текущий целевой угол по Y
    
    private Vector3 dragStartWorldPos;
    private bool isDragging;
    private bool isRotating;
    private Camera cam;

    private void OnEnable()
    {
        mouseClickAction.Enable();
        mousePositionAction.Enable();
        mouseScrollAction.Enable();
        rotateClickAction.Enable();
        mouseDeltaAction.Enable();

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

        mouseClickAction.Disable();
        mousePositionAction.Disable();
        mouseScrollAction.Disable();
        rotateClickAction.Disable();
        mouseDeltaAction.Disable();
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        targetPosition = transform.position;
        targetRotationY = transform.rotation.eulerAngles.y;
        targetRotation = Quaternion.Euler(45f, targetRotationY, 0f);
        
        transform.rotation = targetRotation;
    }

    void Update()
    {
        HandleZoom();
        HandleRotation();
        HandleMouseDrag();
        SmoothMove();
    }

    private void OnDragStarted(InputAction.CallbackContext context)
    {
        if (isRotating) return; // Не перетаскиваем во время поворота

        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            isDragging = true;
            dragStartWorldPos = hit.point;
        }
    }

    private void OnDragCanceled(InputAction.CallbackContext context)
    {
        isDragging = false;
    }

    private void OnRotationStarted(InputAction.CallbackContext context)
    {
        isRotating = true;
        isDragging = false; // Отключаем драг при начале вращения
    }

    private void OnRotationCanceled(InputAction.CallbackContext context)
    {
        isRotating = false;
    }

    private void HandleMouseDrag()
    {
        if (!isDragging || isRotating) return;

        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        
        // Используем текущий целевой поворот (targetRotation) для точного расчета луча без задержек
        Matrix4x4 cameraTargetMatrix = Matrix4x4.TRS(targetPosition, targetRotation, transform.localScale);
        Vector3 localRayStart = cam.transform.InverseTransformPoint(cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane)));
        Vector3 localRayDir = cam.transform.InverseTransformDirection(cam.ScreenPointToRay(mousePos).direction);
        
        Vector3 rayStart = cameraTargetMatrix.MultiplyPoint3x4(localRayStart);
        Vector3 rayDir = cameraTargetMatrix.MultiplyVector(localRayDir);

        Plane groundPlane = new Plane(Vector3.up, dragStartWorldPos);
        Ray ray = new Ray(rayStart, rayDir);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 currentMouseWorldPos = ray.GetPoint(enter);
            Vector3 difference = dragStartWorldPos - currentMouseWorldPos;
            
            targetPosition.x += difference.x;
            targetPosition.z += difference.z;

            ClampPosition();
        }
    }

    private void HandleRotation()
    {
        if (!isRotating) return;

        Vector2 mouseDelta = mouseDeltaAction.ReadValue<Vector2>();
        if (mouseDelta.x == 0) return;

        // 1. Находим точку на земле в центре экрана (pivot вращения)
        Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 pivotPoint = Vector3.zero;

        if (Physics.Raycast(centerRay, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            pivotPoint = hit.point;
        }
        else
        {
            // На случай, если центр экрана смотрит в пустоту, берем математическую плоскость y = 0
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(centerRay, out float enter))
            {
                pivotPoint = centerRay.GetPoint(enter);
            }
        }

        // 2. Рассчитываем новый угол поворота
        float angleValue = mouseDelta.x * rotationSpeed;
        targetRotationY += angleValue;
        targetRotation = Quaternion.Euler(45f, targetRotationY, 0f);

        // 3. Поворачиваем позицию targetPosition вокруг pivotPoint
        targetPosition = pivotPoint + Quaternion.Euler(0f, angleValue, 0f) * (targetPosition - pivotPoint);
        
        ClampPosition();
    }

    private void HandleZoom()
    {
        Vector2 scrollValue = mouseScrollAction.ReadValue<Vector2>();
        if (scrollValue.y == 0) return;

        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hitBeforeZoom, Mathf.Infinity, groundLayer))
        {
            Vector3 worldTargetPos = hitBeforeZoom.point;

            float zoomDirection = -Mathf.Sign(scrollValue.y);
            // Направление зума теперь зависит от текущего разворота камеры
            Vector3 zoomVector = targetRotation * Vector3.forward * (zoomDirection * zoomSpeed);

            Vector3 nextTargetPos = targetPosition + zoomVector;
            nextTargetPos.y = Mathf.Clamp(nextTargetPos.y, minHeight, maxHeight);

            if (Mathf.Approximately(nextTargetPos.y, targetPosition.y)) return;

            float currentYDist = targetPosition.y - worldTargetPos.y;
            float nextYDist = nextTargetPos.y - worldTargetPos.y;
            float scaleFactor = nextYDist / currentYDist;

            targetPosition.x = worldTargetPos.x + (targetPosition.x - worldTargetPos.x) * scaleFactor;
            targetPosition.z = worldTargetPos.z + (targetPosition.z - worldTargetPos.z) * scaleFactor;
            targetPosition.y = nextTargetPos.y;

            ClampPosition();
        }
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
}
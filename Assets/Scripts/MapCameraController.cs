using UnityEngine;
using UnityEngine.InputSystem;

public class MapCameraController : MonoBehaviour
{
    [Header("Настройки ввода")]
    [SerializeField] private InputAction mouseClickAction;
    [SerializeField] private InputAction mousePositionAction;
    [SerializeField] private InputAction mouseScrollAction;
    [SerializeField] private InputAction rotateClickAction;
    [SerializeField] private InputAction mouseDeltaAction;

    [Header("Настройки движения")]
    [SerializeField] private float smoothness = 10f;

    [Header("Настройки поворота")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private float rotationSmoothness = 5f;

    [Header("Настройки зума")]
    [SerializeField] private float zoomSpeed = 15f; // Немного увеличили для отзывчивости плавного зума
    [SerializeField] private float zoomSmoothness = 10f; // Плавность приближения
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 50f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ограничения по осям X/Z")]
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float targetRotationY;
    private float targetHeight; // Целевая высота для плавного зума

    private Vector3 dragStartWorldPos;
    private Vector3 fixedRotationPivot;
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
        targetHeight = targetPosition.y; // Инициализируем целевую высоту
        targetRotationY = transform.rotation.eulerAngles.y;
        targetRotation = Quaternion.Euler(45f, targetRotationY, 0f);

        transform.rotation = targetRotation;
    }

    // ВАЖНО: Перенесли в Update для плавной обработки мыши и Input System
    void Update()
    {
        HandleZoom();
        HandleRotation();
        HandleMouseDrag();
        SmoothMove();
    }

    private void OnDragStarted(InputAction.CallbackContext context)
    {
        if (isRotating) return;

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
        isDragging = false;

        Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(centerRay, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            fixedRotationPivot = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(centerRay, out float enter))
            {
                fixedRotationPivot = centerRay.GetPoint(enter);
            }
        }
    }

    private void OnRotationCanceled(InputAction.CallbackContext context)
    {
        isRotating = false;
    }

    private void HandleMouseDrag()
    {
        if (!isDragging || isRotating) return;

        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();

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

        // 1. Рассчитываем изменение угла, умножая на Time.deltaTime для независимости от FPS.
        // Увеличьте базовый rotationSpeed в инспекторе (например, до 10-20), так как deltaTime уменьшит значение.
        float angleDelta = mouseDelta.x * rotationSpeed * Time.deltaTime;

        targetRotationY += angleDelta;
        targetRotation = Quaternion.Euler(45f, targetRotationY, 0f);

        // 2. Поворачиваем целевую позицию вокруг статичного пивота
        targetPosition = fixedRotationPivot + Quaternion.Euler(0f, angleDelta, 0f) * (targetPosition - fixedRotationPivot);

        ClampPosition();
    }

    private void HandleZoom()
    {
        Vector2 scrollValue = mouseScrollAction.ReadValue<Vector2>();

        // 1. Изменяем целевую высоту
        if (scrollValue.y != 0)
        {
            float zoomDirection = -Mathf.Sign(scrollValue.y);
            targetHeight += zoomDirection * zoomSpeed;
            targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);
        }

        // 2. Плавно интерполируем высоту
        float newHeight = Mathf.Lerp(targetPosition.y, targetHeight, Time.deltaTime * zoomSmoothness);

        // 3. Смещаем X и Z строго по направлению взгляда камеры
        if (!Mathf.Approximately(newHeight, targetPosition.y))
        {
            float heightDelta = newHeight - targetPosition.y;

            // Получаем горизонтальный вектор направления камеры (куда она смотрит на плоскости XZ)
            Vector3 forwardXZ = targetRotation * Vector3.forward;
            forwardXZ.y = 0;
            forwardXZ.Normalize();

            // ИСПРАВЛЕНО: Меняем знак heightDelta на минус (-heightDelta). 
            // Теперь при уменьшении высоты (минус) камера смещается ВПЕРЕД (плюс), 
            // что дает идеальное движение вдоль оси Z камеры вглубь экрана.
            targetPosition += forwardXZ * (-heightDelta);
            targetPosition.y = newHeight;

            ClampPosition();
        }
    }


    private void ClampPosition()
    {
        targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        // ИСПРАВЛЕНО: Теперь ограничивается координата .z, а не .y
        targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
    }

    private void SmoothMove()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothness);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothness);
    }
}

using UnityEngine;

public class CameraMovement
{
    private readonly Camera _cam;
    private readonly LayerMask _groundLayer;
    private Vector3 _dragStartWorldPos;

    public CameraMovement(Camera cam, LayerMask groundLayer)
    {
        _cam = cam;
        _groundLayer = groundLayer;
    }

    public void StartDrag(Vector2 mousePosition)
    {
        Ray ray = _cam.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundLayer))
        {
            _dragStartWorldPos = hit.point;
        }
    }

    public Vector3 CalculateDragDelta(Vector2 mousePosition, Vector3 targetPosition, Quaternion targetRotation)
    {
        // Возвращаем целевую матрицу — она гарантирует отсутствие эффекта желе
        Matrix4x4 cameraTargetMatrix = Matrix4x4.TRS(targetPosition, targetRotation, Vector3.one);

        Vector3 localRayStart = _cam.transform.InverseTransformPoint(_cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, _cam.nearClipPlane)));
        Vector3 localRayDir = _cam.transform.InverseTransformDirection(_cam.ScreenPointToRay(mousePosition).direction);

        Vector3 rayStart = cameraTargetMatrix.MultiplyPoint3x4(localRayStart);
        Vector3 rayDir = cameraTargetMatrix.MultiplyVector(localRayDir);

        Plane groundPlane = new Plane(Vector3.up, _dragStartWorldPos);
        Ray ray = new Ray(rayStart, rayDir);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 currentMouseWorldPos = ray.GetPoint(enter);
            return _dragStartWorldPos - currentMouseWorldPos;
        }

        return Vector3.zero;
    }
}

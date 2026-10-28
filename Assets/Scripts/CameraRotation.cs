using UnityEngine;

public class CameraRotation
{
    private readonly Camera _cam;
    private readonly LayerMask _groundLayer;
    private Vector3 _fixedRotationPivot;

    public CameraRotation(Camera cam, LayerMask groundLayer)
    {
        _cam = cam;
        _groundLayer = groundLayer;
    }

    public void LockPivot()
    {
        Ray centerRay = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(centerRay, out RaycastHit hit, Mathf.Infinity, _groundLayer))
        {
            _fixedRotationPivot = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(centerRay, out float enter))
            {
                _fixedRotationPivot = centerRay.GetPoint(enter);
            }
        }
    }

    public void CalculateRotation(float mouseDeltaX, float rotationSpeed, ref float targetRotationY, ref Quaternion targetRotation, ref Vector3 targetPosition, bool isOrtho)
    {
        if (mouseDeltaX == 0) return;

        float angleDelta = mouseDeltaX * rotationSpeed * Time.deltaTime;
        targetRotationY += angleDelta;

        float targetPitch = isOrtho ? 90f : 45f;
        targetRotation = Quaternion.Euler(targetPitch, targetRotationY, 0f);

        targetPosition = _fixedRotationPivot + Quaternion.Euler(0f, angleDelta, 0f) * (targetPosition - _fixedRotationPivot);
    }
}

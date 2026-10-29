using UnityEngine;

public class CameraZoom
{
    private readonly Camera _cam;
    private float _targetHeight;
    private float _targetOrthoSize;

    public CameraZoom(Camera cam, float initialHeight)
    {
        _cam = cam;
        _targetHeight = initialHeight;
        _targetOrthoSize = initialHeight;
    }

    public void SetTargets(float value)
    {
        _targetHeight = value;
        _targetOrthoSize = value;
    }

    public void AddZoomInput(float scrollY, bool isOrtho, float zoomSpeed, float orthoZoomSpeed, float minHeight, float maxHeight)
    {
        if (scrollY == 0) return;
        float zoomDirection = -Mathf.Sign(scrollY);

        if (isOrtho)
        {
            _targetOrthoSize += zoomDirection * orthoZoomSpeed;
            _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, minHeight, maxHeight);
        }
        else
        {
            _targetHeight += zoomDirection * zoomSpeed;
            _targetHeight = Mathf.Clamp(_targetHeight, minHeight, maxHeight);
        }
    }

    public void ExecuteZoom(ref Vector3 targetPosition, Quaternion targetRotation, bool isOrtho, float zoomSmoothness, float minHeight, float maxHeight)
    {
        if (isOrtho)
        {
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetOrthoSize, Time.deltaTime * zoomSmoothness);
        }
        else
        {
            float newHeight = Mathf.Lerp(targetPosition.y, _targetHeight, Time.deltaTime * zoomSmoothness);

            if (!Mathf.Approximately(newHeight, targetPosition.y))
            {
                float heightDelta = newHeight - targetPosition.y;

                Vector3 forwardXZ = targetRotation * Vector3.forward;
                forwardXZ.y = 0;
                forwardXZ.Normalize();

                targetPosition += forwardXZ * (-heightDelta);
                targetPosition.y = newHeight;
            }
        }
    }
}

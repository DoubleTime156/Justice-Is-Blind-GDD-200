using UnityEngine;

public class UnlitCamFfollower : MonoBehaviour
{
    private Camera _unlitCam;  
    private Camera _mainCam;   

    void Awake()
    {
        _unlitCam = GetComponent<Camera>();

        _mainCam = Camera.main;
        if (_mainCam == null)
        {
            Debug.LogError("UnlitCamFollower: No Camera tagged 'MainCamera' found in the scene.");
        }
    }

    void LateUpdate()
    {
        if (_unlitCam == null || _mainCam == null) return;

        transform.position = _mainCam.transform.position;
        transform.rotation = _mainCam.transform.rotation;

        _unlitCam.orthographic = _mainCam.orthographic;
        if (_unlitCam.orthographic)
        {
            _unlitCam.orthographicSize = _mainCam.orthographicSize;
        }

        _unlitCam.nearClipPlane = _mainCam.nearClipPlane;
        _unlitCam.farClipPlane = _mainCam.farClipPlane;
    }
}

using UnityEngine;

public class unlitCamSetup : MonoBehaviour
{
    public FogManager fog;   

    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void LateUpdate()
    {
        if (fog == null || cam == null) return;

        Vector2 worldMin = fog.worldMin;
        Vector2 worldMax = fog.worldMax;   

    
        Vector2 center = (worldMin + worldMax) * 0.5f;
        transform.position = new Vector3(center.x, center.y, transform.position.z);

        float worldHeight = worldMax.y - worldMin.y; 
        cam.orthographicSize = worldHeight / 2f;     

        cam.nearClipPlane = -50f;
        cam.farClipPlane = 50f;
    }
}


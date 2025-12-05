using UnityEngine;

public class unlitCamSetup : MonoBehaviour
{
    public FogManager fogManager;
    private Camera unlitCam;

    public Vector2 worldMin;
    public Vector2 worldMax;

    private float x;
    private float y;
    private float z = -10;

   


    private void Awake()
    {
        Setup();

    }

    private void Setup()
    {
        unlitCam = GetComponent<Camera>();
        if (!fogManager)
        {
            fogManager = FindFirstObjectByType<FogManager>();
        }

        if (!fogManager || !unlitCam)
        {
            Debug.Log("Create or assign an unlitCam and Fogmanager or the fog will not work!");
            return;
        }

        Vector2 worldMin = fogManager.worldMin;
        Vector2 worldMax = fogManager.worldMax;

        Vector2 center = (worldMin + worldMax) / 2;

        float height = worldMax.y - worldMin.y;
        float z = -10f;
        transform.position = new Vector3(center.x, center.y, z);

        if (unlitCam.orthographic)
        {
            unlitCam.orthographicSize = (height) * 0.5f;
        }
    }

    private void OnValidate()
    {
        Setup();
    }
}

using UnityEngine;

public class CaneTap : MonoBehaviour
{
    public FogManager fogManager;

    [Header("Tap Settings")]
    public float tapRadiusWorld = 6f;    // how big the TRUE-vision bubble is
    public float tapDuration = 1.0f;     // how long it clears vision
    public float tapFalloff = 0.5f;      // edge softness

    [Header("2D Mode")]
    public bool is2D = true;
    public Camera cam;                   // assign main camera

    void Reset()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 worldTap;

            if (is2D)
            {
                // 2D: convert mouse to world (XY)
                var m = Input.mousePosition;
                var w = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, 0f));
                worldTap = new Vector2(w.x, w.y);
            }
            else
            {
                // 3D: raycast to a ground plane at y=0 (adjust to your level)
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                float t;
                Plane ground = new Plane(Vector3.up, Vector3.zero); // y=0
                if (ground.Raycast(ray, out t))
                {
                    Vector3 hit = ray.GetPoint(t);
                    worldTap = new Vector2(hit.x, hit.z); // assuming XZ ground
                }
                else
                {
                    return;
                }
            }

            fogManager.TriggerVisionBurstAt(worldTap, tapRadiusWorld, tapDuration, tapFalloff);
            Debug.Log($"Cane tap at {worldTap}, r={tapRadiusWorld}, t={tapDuration}");
        }
    }
}

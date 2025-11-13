using UnityEngine;

public class NoiseReveal : MonoBehaviour
{
    public FogManager fogManager;
    public float holdSeconds = 0.8f;
    public float falloffWorld = 0.75f;

    public void RevealAtWorld(Vector2 worldPosition, float radiusWorld)
    {
        if (fogManager == null) return;
        fogManager.TriggerVisionBurstAt(worldPosition, Mathf.Max(0f, radiusWorld), Mathf.Max(0.0001f, holdSeconds), Mathf.Max(1e-6f, falloffWorld));
    }

    public void RevealAtUV(Vector2 worldPosition, float radiusUV)
    {
        if (fogManager == null) return;
        Vector2 size = fogManager.worldMax - fogManager.worldMin;
        float minW = Mathf.Max(0.0001f, Mathf.Min(size.x, size.y));
        RevealAtWorld(worldPosition, Mathf.Clamp01(radiusUV) * minW);
    }
}

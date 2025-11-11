using UnityEngine;

public class NoiseReveal : MonoBehaviour
{
    public FogManager fogManager;
    public float holdSeconds = 0.8f;
    public float falloffWorld = 0.5f;

    public void RevealAtWorld(Vector2 worldPosition, float radiusWorld)
    {
        if (fogManager == null) return;
        float edge = ComputeSafeEdgeWorld();
        fogManager.TriggerVisionBurstAt(worldPosition, Mathf.Max(0f, radiusWorld), Mathf.Max(0.0001f, holdSeconds), edge);
    }

    public void RevealAtUV(Vector2 worldPosition, float radiusUV)
    {
        if (fogManager == null) return;
        Vector2 size = fogManager.worldMax - fogManager.worldMin;
        float minW = Mathf.Max(0.0001f, Mathf.Min(size.x, size.y));
        RevealAtWorld(worldPosition, Mathf.Clamp01(radiusUV) * minW);
    }

    float ComputeSafeEdgeWorld()
    {
        Vector2 size = fogManager.worldMax - fogManager.worldMin;
        float minW = Mathf.Max(0.0001f, Mathf.Min(size.x, size.y));
        float texelW = minW / Mathf.Max(1, fogManager.rtSize);
        return Mathf.Max(falloffWorld, 1.5f * texelW);
    }
}

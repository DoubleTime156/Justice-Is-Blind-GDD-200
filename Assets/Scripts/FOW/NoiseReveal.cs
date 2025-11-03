using UnityEngine;

public class NoiseReveal : MonoBehaviour
{
    public FogManager fogManager;

    public float revealRadiusUV = 0.05f;

    public float fullBrightTime = 0.5f;

    public float fadeDuration = 2f;

    [Range(0f, 1f)] public float whiteLevel = 1f;  
    [Range(0f, 1f)] public float grayLevel = 0.3f; 

    private float timer;
    private bool isRevealing;
    private Vector2 worldPos;
    private float currentRadiusUV;

    public void RevealAtWorld(Vector2 worldPosition, float radiusWorld)
    {
        if (fogManager == null) return;
        Vector2 size = fogManager.worldMax - fogManager.worldMin;
        float denom = Mathf.Max(0.0001f, Mathf.Min(size.x, size.y));
        float rUV = Mathf.Clamp01(radiusWorld / denom);
        RevealAtUV(worldPosition, rUV);
    }

    public void RevealAtUV(Vector2 worldPosition, float radiusUV)
    {
        if (fogManager == null) return;

        worldPos = worldPosition;
        currentRadiusUV = Mathf.Clamp01(radiusUV);
        timer = 0f;
        isRevealing = true;

        fogManager.EnqueueReveal(worldPos, currentRadiusUV, whiteLevel, 0f, brightenOnly: true);
        fogManager.FlushNow();
    }

    void Update()
    {
        if (!isRevealing) return;

        timer += Time.deltaTime;

        if (timer < fullBrightTime)
        {
            fogManager.EnqueueReveal(worldPos, currentRadiusUV, whiteLevel, 0f, brightenOnly: true);
            fogManager.FlushNow();
            return;
        }

        float t = Mathf.Clamp01((timer - fullBrightTime) / fadeDuration);
        float intensity = Mathf.Lerp(whiteLevel, grayLevel, t);

        fogManager.EnqueueReveal(worldPos, currentRadiusUV, intensity, 0f, brightenOnly: false);
        fogManager.FlushNow();

        if (t >= 1f) isRevealing = false;
    }
}

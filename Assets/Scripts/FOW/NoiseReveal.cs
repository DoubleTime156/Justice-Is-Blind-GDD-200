using UnityEngine;

public class NoiseReveal : MonoBehaviour
{
    public FogManager fogManager;
    public float revealRadiusUV = 0.05f;
    public float fullBrightTime = 0.5f;
    public float fadeDuration = 2f;

    private float timer = 0f;
    private bool isRevealing = false;
    private Vector2 worldPos;

    // ----------------------------------------------------------------------

    public void RevealAt(Vector2 worldPosition, float radiusUV)
    {
        if (fogManager == null) return;

        worldPos = worldPosition;
        revealRadiusUV = radiusUV;
        timer = 0f;
        isRevealing = true;

        // Immediate full white reveal
        fogManager.EnqueueReveal(worldPos, revealRadiusUV, 1f, 0.02f);
        fogManager.FlushNow(); // force paint this frame
    }

    void Update()
    {
        if (!isRevealing) return;

        timer += Time.deltaTime;

        if (timer > fullBrightTime)
        {
            float t = Mathf.Clamp01((timer - fullBrightTime) / fadeDuration);
            float intensity = Mathf.Lerp(1f, 0.3f, t);

            fogManager.EnqueueReveal(worldPos, revealRadiusUV, intensity, 0.02f);
            fogManager.FlushNow();

            if (t >= 1f)
                isRevealing = false;
        }
    }
}

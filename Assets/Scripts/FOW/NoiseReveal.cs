using UnityEngine;

public class NoiseReveal : MonoBehaviour
{
    public FogManager fogManager;

    [Header("Reveal Settings")]
    public float revealRadiusUV = 0.05f;    // in UV space (relative to map)
    public float fullBrightTime = 0.5f;     // how long it stays white
    public float fadeDuration = 2f;         // how long to fade to gray

    private float timer;
    private bool isRevealing;
    private Vector2 worldPos;

    public void RevealAt(Vector2 worldPosition, float radiusUV)
    {
        if (fogManager == null) return;

        worldPos = worldPosition;
        revealRadiusUV = radiusUV;
        timer = 0f;
        isRevealing = true;

        // 🔹 first frame = full bright white
        fogManager.EnqueueReveal(worldPos, revealRadiusUV, 1f, 0.02f);
        fogManager.FlushNow();  // immediately apply it
    }

    void Update()
    {
        if (!isRevealing) return;

        timer += Time.deltaTime;

        // stay fully bright for a bit
        if (timer < fullBrightTime)
            return;

        // then start fading from white → gray
        float t = Mathf.Clamp01((timer - fullBrightTime) / fadeDuration);
        float intensity = Mathf.Lerp(1f, 0.3f, t);

        fogManager.EnqueueReveal(worldPos, revealRadiusUV, intensity, 0.02f);
        fogManager.FlushNow();

        if (t >= 1f)
            isRevealing = false;
    }
}

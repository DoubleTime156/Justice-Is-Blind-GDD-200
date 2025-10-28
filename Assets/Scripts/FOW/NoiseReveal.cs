using UnityEngine;

public class NoiseReveal : MonoBehaviour
{
    public FogManager fogManager;       // Reference to main fog system
    public Material paintMaterial;      //fogManager paint material

    public float revealRadiusUV = 0.05f;
    public float fullBrightTime = 0.5f;
    public float fadeDuration = 2f;

    private float timer = 0f;
    private bool isRevealing = false;
    private Vector2 uvPos;
    private RenderTexture fogTex;


    public void RevealAt(Vector2 worldPos, float radiusUV)
    {
        if (fogManager == null) return;

        fogTex = fogManager.GetFogMemory();
        paintMaterial = fogManager.fogPainterMaterial;
        revealRadiusUV = radiusUV;

        Vector2 worldMin = fogManager.worldMin;
        Vector2 worldMax = fogManager.worldMax;
        Vector2 worldSize = worldMax - worldMin;

        uvPos = (worldPos - worldMin);
        uvPos.x = worldSize.x != 0 ? uvPos.x / worldSize.x : 0f;
        uvPos.y = worldSize.y != 0 ? uvPos.y / worldSize.y : 0f;
        uvPos = new Vector2(Mathf.Clamp01(uvPos.x), Mathf.Clamp01(uvPos.y));

        PaintFog(1f);

        timer = 0f;
        isRevealing = true;
    }

    void Update()
    {
        if (!isRevealing) return;

        timer += Time.deltaTime;

        if (timer > fullBrightTime)
        {
            float t = (timer - fullBrightTime) / fadeDuration;
            t = Mathf.Clamp01(t);

            float intensity = Mathf.Lerp(1f, 0.3f, t);
            PaintFog(intensity);

            if (t >= 1f)
            {
                isRevealing = false;
                Destroy(this); 
            }
        }
    }

    private void PaintFog(float strength)
    {
        paintMaterial.SetVector("_Position", new Vector4(uvPos.x, uvPos.y, 0, 0));
        paintMaterial.SetFloat("_Radius", revealRadiusUV);

        RenderTexture temp = RenderTexture.GetTemporary(fogTex.width, fogTex.height, 0, fogTex.format);
        Graphics.Blit(fogTex, temp);
        Graphics.Blit(temp, fogTex, paintMaterial);
        RenderTexture.ReleaseTemporary(temp);
    }
}

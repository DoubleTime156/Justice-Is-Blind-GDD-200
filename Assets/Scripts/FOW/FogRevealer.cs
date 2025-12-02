using UnityEngine;

public class FogRevealer : MonoBehaviour
{
    public FogManager fogManager;

    // Size of revealed area
    public float revealRadiusUV = 0.05f;
    // Time it stays fully visible before fading
    public float fullRevealDuration = 2f;
    // Time it takes to fade to gray fog
    public float fadeDuration = 3f;         

    private bool isRevealing = false;
    private float timer;
    private Vector2 uvPos;

    private Material paintMat;
    private RenderTexture fogTex;

    public void TriggerReveal()
    {
        if (fogManager == null) return;

        paintMat = fogManager.fogPainterMaterial;
        fogTex = fogManager.GetFogMemory();

        Vector2 worldPos = new Vector2(transform.position.x, transform.position.y);
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

   

    void PaintFog(float strength)
    {
        paintMat.SetVector("_Position", new Vector4(uvPos.x, uvPos.y, 0, 0));
        paintMat.SetFloat("_Radius", revealRadiusUV);

        Shader.SetGlobalFloat("_FogRevealIntensity", strength);

        RenderTexture temp = RenderTexture.GetTemporary(fogTex.width, fogTex.height, 0, fogTex.format);
        Graphics.Blit(fogTex, temp);
        Graphics.Blit(temp, fogTex, paintMat);
        RenderTexture.ReleaseTemporary(temp);
    }
}

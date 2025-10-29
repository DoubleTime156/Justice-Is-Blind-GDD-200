using UnityEngine;
public class NewFogManager : MonoBehaviour
{
    public Material fogPainterMaterial; // draws reveals into fogMemory
    public Material fogDisplayMaterial; // final fog render
    public Transform player;

    public int rtSize = 1024;
    private RenderTexture fogMemory;
    private RenderTexture fogFrame;

    public Vector2 worldMin = new Vector2(-100f, -100f);
    public Vector2 worldMax = new Vector2(100f, 100f);

    [Range(0f, 1f)] public float fadeSpeed = 0.01f;

    private static readonly int PositionID = Shader.PropertyToID("_Position");
    private static readonly int RadiusID = Shader.PropertyToID("_Radius");
    private static readonly int FogTexID = Shader.PropertyToID("_MainTex");

    void Start()
    {
        InitializeRenderTextures();
        PushWorldParamsToMaterials();
        InitializeRenderTextures();
        PushWorldParamsToMaterials();
        Debug.Log($"FogMemory Created: {fogMemory.width}x{fogMemory.height}");
    }

    void InitializeRenderTextures()
    {
        if (fogMemory != null) fogMemory.Release();
        if (fogFrame != null) fogFrame.Release();

        fogMemory = new RenderTexture(rtSize, rtSize, 0, RenderTextureFormat.ARGB32);
        fogFrame = new RenderTexture(rtSize, rtSize, 0, RenderTextureFormat.ARGB32);

        fogMemory.Create();
        fogFrame.Create();

        // Start black = unseen
        RenderTexture.active = fogMemory;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
    }

    void PushWorldParamsToMaterials()
    {
        fogDisplayMaterial.SetTexture(FogTexID, fogMemory);
        fogDisplayMaterial.SetTexture(FogTexID, fogMemory);
        Debug.Log("Fog texture assigned to FogDisplay material");
    }

    void Update()
    {
        // Each frame, slowly fade memory (simulate visual memory decay)
        fogPainterMaterial.SetFloat("_FadeSpeed", fadeSpeed);
        Graphics.Blit(fogMemory, fogFrame, fogPainterMaterial, 0);
        Graphics.Blit(fogFrame, fogMemory);

        if (player != null)
        {
            RevealCircle(player.position, 0.05f);
        }
    }

    /// <summary>
    /// Creates a circular reveal in the fog memory.
    /// </summary>
    public void RevealCircle(Vector2 worldPos, float radius)
    {
        Vector2 uv = WorldToUV(worldPos);
        fogPainterMaterial.SetVector(PositionID, new Vector4(uv.x, uv.y, 0, 0));
        fogPainterMaterial.SetFloat(RadiusID, radius);

        RenderTexture temp = RenderTexture.GetTemporary(rtSize, rtSize, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(fogMemory, temp);
        Graphics.Blit(temp, fogMemory, fogPainterMaterial, 1);
        RenderTexture.ReleaseTemporary(temp);
    }

    Vector2 WorldToUV(Vector2 worldPos)
    {
        return new Vector2(
            Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x),
            Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.y)
        );
    }

    void OnDestroy()
    {
        fogMemory?.Release();
        fogFrame?.Release();
    }
}

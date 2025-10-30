using UnityEngine;
using System.Collections.Generic;

public class FogManager : MonoBehaviour
{
    [Header("Refs")]
    public PlayerPosition playerVision;
    public Material fogPainterMaterial;   // uses Custom/FogPainterOverwrite
    public Material fogDisplayMaterial;   // uses Custom/2DCircleVision
    public Transform player;

    [Header("RT")]
    public int rtSize = 2048;
    private RenderTexture fogMemory;
    private RenderTexture fogScratch;

    [Header("World Bounds")]
    public Vector2 worldMin = new Vector2(-100f, -100f);
    public Vector2 worldMax = new Vector2(100f, 100f);

    struct RevealReq { public Vector2 uv; public float radiusUV; public float intensity; public float edge; }
    private readonly List<RevealReq> _queue = new();

    static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    static readonly int PositionID = Shader.PropertyToID("_Position");
    static readonly int RadiusID = Shader.PropertyToID("_Radius");
    static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    static readonly int EdgeID = Shader.PropertyToID("_Edge");

    void Start()
    {
        InitializeRenderTextures();
        PushWorldParamsToMaterials();
        ClearFog();
    }

    void OnDestroy()
    {
        if (fogMemory) { fogMemory.Release(); Destroy(fogMemory); }
        if (fogScratch) { fogScratch.Release(); Destroy(fogScratch); }
    }

    void InitializeRenderTextures()
    {
        if (fogMemory) { fogMemory.Release(); Destroy(fogMemory); }
        if (fogScratch) { fogScratch.Release(); Destroy(fogScratch); }

        fogMemory = new RenderTexture(rtSize, rtSize, 0, RenderTextureFormat.ARGB32);
        fogScratch = new RenderTexture(rtSize, rtSize, 0, RenderTextureFormat.ARGB32);

        fogMemory.filterMode = FilterMode.Bilinear;
        fogScratch.filterMode = FilterMode.Bilinear;
        fogMemory.wrapMode = fogScratch.wrapMode = TextureWrapMode.Clamp;

        fogMemory.Create();
        fogScratch.Create();

        if (fogDisplayMaterial) fogDisplayMaterial.SetTexture("_FogTex", fogMemory);
    }

    void PushWorldParamsToMaterials()
    {
        Vector2 worldSize = worldMax - worldMin;
        if (fogPainterMaterial)
        {
            fogPainterMaterial.SetVector("_WorldMin", new Vector4(worldMin.x, worldMin.y, 0, 0));
            fogPainterMaterial.SetVector("_WorldSize", new Vector4(worldSize.x, worldSize.y, 0, 0));
        }
        if (fogDisplayMaterial)
        {
            fogDisplayMaterial.SetVector("_WorldMin", new Vector4(worldMin.x, worldMin.y, 0, 0));
            fogDisplayMaterial.SetVector("_WorldSize", new Vector4(worldSize.x, worldSize.y, 0, 0));
        }
    }

    public RenderTexture GetFogMemory() => fogMemory;

    public void ClearFog()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = fogMemory;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;
    }

    public void EnqueueReveal(Vector2 worldPos, float radiusUV, float intensity = 1f, float edge = 0.02f)
    {
        Vector2 uv = WorldToUV(worldPos);
        _queue.Add(new RevealReq { uv = uv, radiusUV = radiusUV, intensity = intensity, edge = edge });
    }

    Vector2 WorldToUV(Vector2 worldPos)
    {
        Vector2 worldSize = worldMax - worldMin;
        Vector2 uv = (worldPos - worldMin);
        uv.x = worldSize.x != 0 ? uv.x / worldSize.x : 0f;
        uv.y = worldSize.y != 0 ? uv.y / worldSize.y : 0f;
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);
        return uv;
    }

    void LateUpdate()
    {
        if (fogPainterMaterial == null || player == null) { _queue.Clear(); return; }

        // --- Always enqueue player's current view for memory ---
        if (playerVision != null)
        {
            Vector2 worldSize = worldMax - worldMin;
            float radiusUV = playerVision.radius / Mathf.Min(worldSize.x, worldSize.y);
            Vector2 uv = WorldToUV(player.position);

            // Gray memory circle slightly larger
            _queue.Add(new RevealReq
            {
                uv = uv,
                radiusUV = radiusUV * 1.05f,
                intensity = 0.3f,
                edge = 0.02f
            });

            // White live circle
            _queue.Add(new RevealReq
            {
                uv = uv,
                radiusUV = radiusUV,
                intensity = 1f,
                edge = 0.02f
            });
        }

        if (_queue.Count == 0) return;

        Graphics.Blit(fogMemory, fogScratch);

        for (int i = 0; i < _queue.Count; i++)
        {
            var r = _queue[i];
            fogPainterMaterial.SetVector(PositionID, new Vector4(r.uv.x, r.uv.y, 0, 0));
            fogPainterMaterial.SetFloat(RadiusID, r.radiusUV);
            fogPainterMaterial.SetFloat(IntensityID, r.intensity);
            fogPainterMaterial.SetFloat(EdgeID, r.edge);
            fogPainterMaterial.SetTexture(MainTexID, fogScratch);
            Graphics.Blit(fogScratch, fogMemory, fogPainterMaterial);
            Graphics.Blit(fogMemory, fogScratch);
        }

        _queue.Clear();
    }
    public void FlushNow()
    {
        // Manually invoke LateUpdate() logic for instant paint
        LateUpdate();
    }

}

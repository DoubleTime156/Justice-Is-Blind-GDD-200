using UnityEngine;
using System.Collections.Generic;

public class FogManager : MonoBehaviour
{
    [Header("Refs")]
    public PlayerPosition playerVision;           // provides player radius & (optionally) falloff
    public Material fogPainterMaterial;           // Shader: Custom/FogPainterOverwrite (or Smooth)
    public Material fogDisplayMaterial;           // Shader: Custom/FogDisplayVision (or 2DCircleVision)
    public Transform player;

    [Header("RT")]
    public int rtSize = 2048;
    private RenderTexture fogMemory;
    private RenderTexture fogScratch;

    [Header("World Bounds (match your map)")]
    public Vector2 worldMin = new Vector2(-100f, -100f);
    public Vector2 worldMax = new Vector2(100f, 100f);

    [Header("Live Vision Look")]
    public float liveFalloff = 0.5f; // how soft the live circle edge looks

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
        fogMemory.wrapMode = TextureWrapMode.Clamp;
        fogScratch.wrapMode = TextureWrapMode.Clamp;

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
        return new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));
    }

    void LateUpdate()
    {
        if (player == null) { _queue.Clear(); return; }

        // 1) DRIVE THE LIVE WHITE CIRCLE (display only — clears all fog visually)
        if (fogDisplayMaterial && playerVision)
        {
            fogDisplayMaterial.SetVector("_PlayerPos", new Vector4(player.position.x, player.position.y, 0, 0));
            fogDisplayMaterial.SetFloat("_Radius", playerVision.radius);
            fogDisplayMaterial.SetFloat("_Falloff", liveFalloff);
        }

        // 2) MEMORY: walking paints ONLY GRAY into the fog texture at EXACT player radius
        if (fogPainterMaterial && playerVision)
        {
            Vector2 worldSize = worldMax - worldMin;
            float radiusUV = playerVision.radius / Mathf.Min(worldSize.x, worldSize.y);
            Vector2 uv = WorldToUV(player.position);

            // Paint gray memory at your current circle (no white for walking)
            _queue.Add(new RevealReq
            {
                uv = uv,
                radiusUV = radiusUV,
                intensity = 0.3f,     // gray memory
                edge = 0.02f
            });
        }

        if (_queue.Count == 0 || fogPainterMaterial == null)
        {
            _queue.Clear();
            return;
        }

        // Apply queued reveals (ping-pong once per frame)
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
            Graphics.Blit(fogMemory, fogScratch); // keep scratch up-to-date
        }

        _queue.Clear();
    }

    // For NoiseReveal to force immediate paint when triggered
    public void FlushNow() => LateUpdate();
}

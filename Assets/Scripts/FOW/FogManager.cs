using System.Collections.Generic;
using UnityEngine;

public class FogManager : MonoBehaviour
{
    [Header("Refs")]
    public PlayerPosition playerVision;           // provides player radius (and optionally falloff)
    public Material fogPainterMaterial;           // Shader that writes to fogMemory (gray/white)
    public Material fogDisplayMaterial;           // Shader that renders overlay using fog + live circles
    public Transform player;

    [Header("RenderTextures")]
    public int rtSize = 2048;
    private RenderTexture fogMemory;
    private RenderTexture fogScratch;

    [Header("World Bounds (match your map)")]
    public Vector2 worldMin = new Vector2(-100f, -100f);
    public Vector2 worldMax = new Vector2(100f, 100f);

    [Header("Live Vision Look (Player Base Circle)")]
    public float liveFalloff = 0.5f;              // world units, soft edge used for the player’s base vision

    // ====== REVEAL QUEUE (for memory painting) ======
    private struct RevealReq { public Vector2 uv; public float radiusUV; public float intensity; public float edge; }
    private readonly List<RevealReq> _queue = new();

    // Shader property IDs for painter
    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int PositionID = Shader.PropertyToID("_Position");
    private static readonly int RadiusID = Shader.PropertyToID("_Radius");
    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int EdgeID = Shader.PropertyToID("_Edge");

    // ====== TRUE-VISION BURSTS (not tied to player) ======
    [Header("Vision Burst (true vision circles not tied to player)")]
    public float defaultBurstFalloff = 0.5f;      // world units (edge softness for bursts)
    [Range(1, 32)]
    public int maxBursts = 8;                     // must match/fit shader’s MAX_BURSTS

    private struct Burst
    {
        public Vector2 worldPos;
        public float radiusWorld;
        public float falloffWorld;
        public float timer;                       // seconds remaining
    }
    private readonly List<Burst> _bursts = new();

    // Shader property IDs for display (arrays + count)
    private static readonly int BurstCountID = Shader.PropertyToID("_BurstCount");
    private static readonly int BurstPosID = Shader.PropertyToID("_BurstPos");
    private static readonly int BurstRadID = Shader.PropertyToID("_BurstRad");

    // ====== LIFECYCLE ======
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

    // ====== INIT ======
    void InitializeRenderTextures()
    {
        if (fogMemory) { fogMemory.Release(); Destroy(fogMemory); }
        if (fogScratch) { fogScratch.Release(); Destroy(fogScratch); }

        fogMemory = new RenderTexture(rtSize, rtSize, 0, RenderTextureFormat.ARGB32);
        fogScratch = new RenderTexture(rtSize, rtSize, 0, RenderTextureFormat.ARGB32);

        // Choose look: Point => hard pixel blocks, Bilinear => softer blocks
        fogMemory.filterMode = FilterMode.Point;
        fogScratch.filterMode = FilterMode.Point;

        fogMemory.wrapMode = TextureWrapMode.Clamp;
        fogScratch.wrapMode = TextureWrapMode.Clamp;

        fogMemory.Create();
        fogScratch.Create();

        if (fogDisplayMaterial)
        {
            fogDisplayMaterial.SetTexture("_FogTex", fogMemory);
            // If your display shader supports quantization toggle, you can set it here:
            // fogDisplayMaterial.SetFloat("_QuantizeCircle", 1f);
        }
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

    // ====== PUBLIC API ======
    public RenderTexture GetFogMemory() => fogMemory;

    public void ClearFog()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = fogMemory;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;
    }

    /// <summary>
    /// Queue a reveal to paint into fogMemory (gray/white memory). worldPos is in WORLD space; radiusUV is in UV space.
    /// </summary>
    public void EnqueueReveal(Vector2 worldPos, float radiusUV, float intensity = 1f, float edge = 0.02f)
    {
        Vector2 uv = WorldToUV(worldPos);
        _queue.Add(new RevealReq { uv = uv, radiusUV = radiusUV, intensity = intensity, edge = edge });
    }

    /// <summary>
    /// Trigger a TRUE-VISION burst (clears overlay, shows enemies) at an arbitrary world position, without painting gray.
    /// </summary>
    public void TriggerVisionBurstAt(Vector2 worldPos, float radiusWorld, float durationSeconds, float falloffWorld = -1f)
    {
        if (falloffWorld < 0f) falloffWorld = defaultBurstFalloff;
        if (_bursts.Count >= maxBursts) _bursts.RemoveAt(0); // drop oldest if full

        _bursts.Add(new Burst
        {
            worldPos = worldPos,
            radiusWorld = Mathf.Max(0f, radiusWorld),
            falloffWorld = Mathf.Max(0.0001f, falloffWorld),
            timer = Mathf.Max(0f, durationSeconds)
        });
    }

    /// <summary>
    /// Forcing immediate application of queued reveals (rarely needed—usually LateUpdate handles it).
    /// </summary>
    public void FlushNow() => LateUpdate();

    // ====== HELPERS ======
    Vector2 WorldToUV(Vector2 worldPos)
    {
        Vector2 worldSize = worldMax - worldMin;
        Vector2 uv = (worldPos - worldMin);
        uv.x = worldSize.x != 0 ? uv.x / worldSize.x : 0f;
        uv.y = worldSize.y != 0 ? uv.y / worldSize.y : 0f;
        return new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));
    }

    // ====== MAIN TICK ======
    void LateUpdate()
    {
        if (player == null)
        {
            _queue.Clear();
            return;
        }

        // --- DISPLAY: set live clear circles (player base + active bursts) ---
        if (fogDisplayMaterial && playerVision)
        {
            // Update burst timers & prune expired
            for (int i = _bursts.Count - 1; i >= 0; --i)
            {
                Burst b = _bursts[i];
                b.timer -= Time.deltaTime;
                if (b.timer <= 0f) _bursts.RemoveAt(i);
                else _bursts[i] = b;
            }

            // Prepare arrays: slot 0 = player base circle, then bursts
            int burstCount = Mathf.Min(maxBursts, 1 + _bursts.Count);

            // Unity requires arrays sized to at least 'burstCount', but they can be larger.
            Vector4[] posArray = new Vector4[Mathf.Max(burstCount, 1)];
            Vector4[] radArray = new Vector4[Mathf.Max(burstCount, 1)]; // x=radius, y=falloff

            // Slot 0: player base circle
            posArray[0] = new Vector4(player.position.x, player.position.y, 0, 0);
            radArray[0] = new Vector4(playerVision.radius, liveFalloff, 0, 0);

            // Fill subsequent slots with bursts
            for (int i = 0; i < burstCount - 1; i++)
            {
                var b = _bursts[i];
                posArray[i + 1] = new Vector4(b.worldPos.x, b.worldPos.y, 0, 0);
                radArray[i + 1] = new Vector4(b.radiusWorld, b.falloffWorld, 0, 0);
            }

            fogDisplayMaterial.SetInt(BurstCountID, burstCount);
            fogDisplayMaterial.SetVectorArray(BurstPosID, posArray);
            fogDisplayMaterial.SetVectorArray(BurstRadID, radArray);

            // Optional legacy compatibility (some older shaders read these):
            fogDisplayMaterial.SetVector("_PlayerPos", new Vector4(player.position.x, player.position.y, 0, 0));
            fogDisplayMaterial.SetFloat("_Radius", playerVision.radius);
            fogDisplayMaterial.SetFloat("_Falloff", liveFalloff);

            // World params are set in PushWorldParamsToMaterials() already
        }

        // --- MEMORY PAINT: gray under the player's BASE radius only (no bursts!) ---
        if (fogPainterMaterial && playerVision)
        {
            Vector2 worldSize = worldMax - worldMin;
            float radiusUV = playerVision.radius / Mathf.Min(worldSize.x, worldSize.y);
            Vector2 uv = WorldToUV(player.position);

            // Paint steady gray trail at current player radius (does not grow with bursts)
            _queue.Add(new RevealReq
            {
                uv = uv,
                radiusUV = radiusUV,
                intensity = 0.3f,      // gray memory level
                edge = 0.02f
            });
        }

        // --- APPLY QUEUED REVEALS (once per frame) ---
        if (_queue.Count > 0 && fogPainterMaterial != null)
        {
            // Ping-pong: start from fogMemory into scratch
            Graphics.Blit(fogMemory, fogScratch);

            for (int i = 0; i < _queue.Count; i++)
            {
                var r = _queue[i];
                fogPainterMaterial.SetVector(PositionID, new Vector4(r.uv.x, r.uv.y, 0, 0));
                fogPainterMaterial.SetFloat(RadiusID, r.radiusUV);
                fogPainterMaterial.SetFloat(IntensityID, r.intensity);
                fogPainterMaterial.SetFloat(EdgeID, r.edge);
                fogPainterMaterial.SetTexture(MainTexID, fogScratch);

                // Write to memory, then keep scratch in sync
                Graphics.Blit(fogScratch, fogMemory, fogPainterMaterial);
                Graphics.Blit(fogMemory, fogScratch);
            }
        }

        _queue.Clear();
    }
}

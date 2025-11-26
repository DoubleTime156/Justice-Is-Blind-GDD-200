using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class FogManager : MonoBehaviour
{
    public PlayerPosition playerVision;
    public Material fogPainterMaterial;
    public Material fogDisplayMaterial;
    public Transform player;

    public int rtSize = 4096;

    RenderTexture fogMemory;
    RenderTexture fogScratch;

    public Vector2 worldMin = new Vector2(-100f, -100f);
    public Vector2 worldMax = new Vector2(100f, 100f);

    public MaskController maskController;
    public RenderTexture liveMaskOverride;

    public float liveFalloff = 0.75f;
    public float memoryAlpha = 0.35f;
    public float memoryIntensity = 0.3f;

    public bool writeLiveToMemory = true;
    public bool writeBurstsToMemory = true;
    public bool writeQueuedToMemory = false;

    public float defaultBurstFalloff = 0.75f;
    [Range(1, 32)] public int maxBursts = 8;

    public float memCoverageBiasTexels = 0.5f;

    struct Burst { public Vector2 pos; public float r; public float f; public float t; }
    readonly List<Burst> bursts = new();

    enum WriteMode { LERP = 0, MAX = 1 }
    struct Q { public Vector2 pos; public float r; public float intensity; public WriteMode mode; }
    readonly List<Q> queue = new();

    static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    static readonly int WorldMinID = Shader.PropertyToID("_WorldMin");
    static readonly int WorldSizeID = Shader.PropertyToID("_WorldSize");
    static readonly int LiveMaskID = Shader.PropertyToID("_LiveMaskTex");
    static readonly int MemWriteIntensityID = Shader.PropertyToID("_MemWriteIntensity");
    static readonly int MemCoverageBiasWorldID = Shader.PropertyToID("_MemCoverageBiasWorld");
    static readonly int WriteLiveID = Shader.PropertyToID("_WriteLive");
    static readonly int WriteBurstsID = Shader.PropertyToID("_WriteBursts");
    static readonly int WriteQueuedID = Shader.PropertyToID("_WriteQueued");
    static readonly int BurstCountID = Shader.PropertyToID("_BurstCount");
    static readonly int BurstPosID = Shader.PropertyToID("_BurstPos");
    static readonly int BurstRadID = Shader.PropertyToID("_BurstRad");
    static readonly int QueuedCountID = Shader.PropertyToID("_QueuedCount");
    static readonly int QueuedPosID = Shader.PropertyToID("_QueuedPos");
    static readonly int QueuedRadID = Shader.PropertyToID("_QueuedRad");
    static readonly int DispLiveMaskID = Shader.PropertyToID("_LiveMaskTex");
    static readonly int DispWorldMinID = Shader.PropertyToID("_WorldMin");
    static readonly int DispWorldSizeID = Shader.PropertyToID("_WorldSize");
    static readonly int DispBurstCountID = Shader.PropertyToID("_BurstCount");
    static readonly int DispBurstPosID = Shader.PropertyToID("_BurstPos");
    static readonly int DispBurstRadID = Shader.PropertyToID("_BurstRad");
    static readonly int DispMemAlphaID = Shader.PropertyToID("_MemoryAlpha");

    void Awake()
    {
        if (maskController == null) maskController = FindObjectOfType<MaskController>();
    }

    void Start()
    {
        var desc = new RenderTextureDescriptor(rtSize, rtSize, RenderTextureFormat.ARGB32, 0)
        { useMipMap = false, autoGenerateMips = false, msaaSamples = 1 };
        fogMemory = new RenderTexture(desc);
        fogScratch = new RenderTexture(desc);
        fogMemory.filterMode = FilterMode.Bilinear;
        fogScratch.filterMode = FilterMode.Bilinear;
        fogMemory.wrapMode = TextureWrapMode.Clamp;
        fogScratch.wrapMode = TextureWrapMode.Clamp;
        fogMemory.Create();
        fogScratch.Create();

        if (fogDisplayMaterial) fogDisplayMaterial.SetTexture("_FogTex", fogMemory);

        Vector2 size = worldMax - worldMin;
        Vector4 minV = new Vector4(worldMin.x, worldMin.y, 0, 0);
        Vector4 sizeV = new Vector4(size.x, size.y, 0, 0);
        if (fogPainterMaterial) { fogPainterMaterial.SetVector(WorldMinID, minV); fogPainterMaterial.SetVector(WorldSizeID, sizeV); }
        if (fogDisplayMaterial) { fogDisplayMaterial.SetVector(DispWorldMinID, minV); fogDisplayMaterial.SetVector(DispWorldSizeID, sizeV); }

        ClearFog();
    }

    void OnDestroy()
    {
        if (fogMemory) { fogMemory.Release(); Destroy(fogMemory); }
        if (fogScratch) { fogScratch.Release(); Destroy(fogScratch); }
    }

    public RenderTexture GetFogMemory() => fogMemory;

    public void ClearFog()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = fogMemory;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;
    }

    public void EnqueueRevealWorld(Vector2 pos, float radiusWorld, float intensity = 1f, float edgeWorld = 0.5f, bool brightenOnly = false)
    {
        queue.Add(new Q { pos = pos, r = Mathf.Max(0f, radiusWorld), intensity = Mathf.Clamp01(intensity), mode = brightenOnly ? WriteMode.MAX : WriteMode.LERP });
    }

    public void TriggerVisionBurstAt(Vector2 pos, float radiusWorld, float durationSeconds, float falloffWorld = -1f)
    {
        if (falloffWorld < 0f) falloffWorld = defaultBurstFalloff;
        if (bursts.Count >= maxBursts) bursts.RemoveAt(0);
        bursts.Add(new Burst { pos = pos, r = Mathf.Max(0f, radiusWorld), f = Mathf.Max(1e-6f, falloffWorld), t = Mathf.Max(0f, durationSeconds) });
    }

    public void FlushNow() => LateUpdate();

    RenderTexture ResolveLiveMaskRT()
    {
        if (liveMaskOverride) return liveMaskOverride;
        if (maskController == null) maskController = FindObjectOfType<MaskController>();
        return maskController ? maskController.LiveMaskRT : null;
    }

    void LateUpdate()
    {
        for (int i = bursts.Count - 1; i >= 0; --i)
        {
            var b = bursts[i]; b.t -= Time.deltaTime;
            if (b.t <= 0f) bursts.RemoveAt(i); else bursts[i] = b;
        }

        if (fogPainterMaterial == null) return;

        Graphics.Blit(fogMemory, fogScratch);

        Vector2 size = worldMax - worldMin;
        float minWorld = Mathf.Min(size.x, size.y);
        float texelWorld = minWorld / Mathf.Max(1, rtSize);
        float covBias = Mathf.Max(0f, memCoverageBiasTexels) * texelWorld;

        var liveMaskRT = ResolveLiveMaskRT();
        Texture liveMaskTex = liveMaskRT != null ? (Texture)liveMaskRT : (Texture)Texture2D.blackTexture;

        fogPainterMaterial.SetTexture(MainTexID, fogScratch);
        fogPainterMaterial.SetTexture(LiveMaskID, liveMaskTex);
        fogPainterMaterial.SetVector(WorldMinID, new Vector4(worldMin.x, worldMin.y, 0, 0));
        fogPainterMaterial.SetVector(WorldSizeID, new Vector4(size.x, size.y, 0, 0));
        fogPainterMaterial.SetFloat(MemWriteIntensityID, Mathf.Clamp01(memoryIntensity));
        fogPainterMaterial.SetFloat(MemCoverageBiasWorldID, covBias);
        fogPainterMaterial.SetInt(WriteLiveID, writeLiveToMemory ? 1 : 0);
        fogPainterMaterial.SetInt(WriteBurstsID, writeBurstsToMemory ? 1 : 0);
        fogPainterMaterial.SetInt(WriteQueuedID, writeQueuedToMemory ? 1 : 0);

        int bc = Mathf.Min(maxBursts, bursts.Count);
        fogPainterMaterial.SetInt(BurstCountID, bc);
        if (bc > 0)
        {
            var posA = new Vector4[bc];
            var radA = new Vector4[bc];
            for (int i = 0; i < bc; i++)
            {
                var b = bursts[i];
                posA[i] = new Vector4(b.pos.x, b.pos.y, 0, 0);
                radA[i] = new Vector4(b.r, b.f, 0, 0);
            }
            fogPainterMaterial.SetVectorArray(BurstPosID, posA);
            fogPainterMaterial.SetVectorArray(BurstRadID, radA);
        }

        int qc = queue.Count;
        fogPainterMaterial.SetInt(QueuedCountID, qc);
        if (qc > 0)
        {
            var qPos = new Vector4[qc];
            var qRad = new Vector4[qc];
            for (int i = 0; i < qc; i++)
            {
                qPos[i] = new Vector4(queue[i].pos.x, queue[i].pos.y, 0, 0);
                qRad[i] = new Vector4(queue[i].r, 0f, queue[i].intensity, queue[i].mode == WriteMode.MAX ? 1f : 0f);
            }
            fogPainterMaterial.SetVectorArray(QueuedPosID, qPos);
            fogPainterMaterial.SetVectorArray(QueuedRadID, qRad);
        }

        Graphics.Blit(fogScratch, fogMemory, fogPainterMaterial);
        queue.Clear();

        if (fogDisplayMaterial)
        {
            fogDisplayMaterial.SetTexture(DispLiveMaskID, liveMaskTex);
            fogDisplayMaterial.SetVector(DispWorldMinID, new Vector4(worldMin.x, worldMin.y, 0, 0));
            fogDisplayMaterial.SetVector(DispWorldSizeID, new Vector4(size.x, size.y, 0, 0));
            fogDisplayMaterial.SetInt(DispBurstCountID, bc);
            if (bc > 0)
            {
                var posA = new Vector4[bc];
                var radA = new Vector4[bc];
                for (int i = 0; i < bc; i++)
                {
                    var b = bursts[i];
                    posA[i] = new Vector4(b.pos.x, b.pos.y, 0, 0);
                    radA[i] = new Vector4(b.r, Mathf.Max(1e-6f, b.f), 0, 0);
                }
                fogDisplayMaterial.SetVectorArray(DispBurstPosID, posA);
                fogDisplayMaterial.SetVectorArray(DispBurstRadID, radA);
            }
            fogDisplayMaterial.SetFloat(DispMemAlphaID, memoryAlpha);
        }
    }
}

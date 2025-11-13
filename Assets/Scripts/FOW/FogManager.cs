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
    private RenderTexture fogMemory;
    private RenderTexture fogScratch;

    public Vector2 worldMin = new Vector2(-100f, -100f);
    public Vector2 worldMax = new Vector2(100f, 100f);

    private float liveFalloff = 0.75f;
    private float memoryAlpha = 0.70f;
    private float memoryIntensity = 0.30f;

    private bool writeLiveToMemory = true;
    private bool writeBurstsToMemory = true;
    private bool writeQueuedToMemory = false;

    public float defaultBurstFalloff = 0.75f;
    [Range(1, 32)] public int maxBursts = 8;

    private float memCoverageBiasTexels = 0.5f;

    private struct Burst { public Vector2 worldPos; public float radiusWorld; public float falloffWorld; public float timer; }
    private readonly List<Burst> _bursts = new();

    private enum WriteMode { LERP = 0, MAX = 1 }
    private struct RevealReq { public Vector2 worldPos; public float radiusWorld; public float intensity; public WriteMode mode; }
    private readonly List<RevealReq> _queue = new();

    static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    static readonly int WorldMinID = Shader.PropertyToID("_WorldMin");
    static readonly int WorldSizeID = Shader.PropertyToID("_WorldSize");
    static readonly int PlayerPosID = Shader.PropertyToID("_PlayerPos");
    static readonly int RadiusID = Shader.PropertyToID("_Radius");
    static readonly int FalloffID = Shader.PropertyToID("_Falloff");
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

    static readonly int DispBurstCountID = Shader.PropertyToID("_BurstCount");
    static readonly int DispBurstPosID = Shader.PropertyToID("_BurstPos");
    static readonly int DispBurstRadID = Shader.PropertyToID("_BurstRad");
    static readonly int DispPlayerPosID = Shader.PropertyToID("_PlayerPos");
    static readonly int DispRadiusID = Shader.PropertyToID("_Radius");
    static readonly int DispFalloffID = Shader.PropertyToID("_Falloff");
    static readonly int DispMemAlphaID = Shader.PropertyToID("_MemoryAlpha");

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
        fogMemory.anisoLevel = 0;
        fogScratch.anisoLevel = 0;
        fogMemory.Create();
        fogScratch.Create();

        if (fogDisplayMaterial) fogDisplayMaterial.SetTexture("_FogTex", fogMemory);

        Vector2 size = worldMax - worldMin;
        Vector4 minV = new Vector4(worldMin.x, worldMin.y, 0, 0);
        Vector4 sizeV = new Vector4(size.x, size.y, 0, 0);
        if (fogPainterMaterial) { fogPainterMaterial.SetVector(WorldMinID, minV); fogPainterMaterial.SetVector(WorldSizeID, sizeV); }
        if (fogDisplayMaterial) { fogDisplayMaterial.SetVector(WorldMinID, minV); fogDisplayMaterial.SetVector(WorldSizeID, sizeV); }

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

    public void EnqueueRevealWorld(Vector2 worldPos, float radiusWorld, float intensity = 1f, float edgeWorld = 0.5f, bool brightenOnly = false)
    {
        _queue.Add(new RevealReq
        {
            worldPos = worldPos,
            radiusWorld = Mathf.Max(0f, radiusWorld),
            intensity = Mathf.Clamp01(intensity),
            mode = brightenOnly ? WriteMode.MAX : WriteMode.LERP
        });
    }

    public void TriggerVisionBurstAt(Vector2 worldPos, float radiusWorld, float durationSeconds, float falloffWorld = -1f)
    {
        if (falloffWorld < 0f) falloffWorld = defaultBurstFalloff;
        if (_bursts.Count >= maxBursts) _bursts.RemoveAt(0);
        _bursts.Add(new Burst
        {
            worldPos = worldPos,
            radiusWorld = Mathf.Max(0f, radiusWorld),
            falloffWorld = Mathf.Max(1e-6f, falloffWorld),
            timer = Mathf.Max(0f, durationSeconds)
        });
    }

    public void FlushNow() => LateUpdate();

    void LateUpdate()
    {
        if (player == null || fogPainterMaterial == null) return;

        for (int i = _bursts.Count - 1; i >= 0; --i)
        {
            var b = _bursts[i]; b.timer -= Time.deltaTime;
            if (b.timer <= 0f) _bursts.RemoveAt(i); else _bursts[i] = b;
        }

        Vector2 size = worldMax - worldMin;
        float minWorld = Mathf.Min(size.x, size.y);
        float texelWorld = minWorld / Mathf.Max(1, rtSize);
        float liveR = playerVision ? playerVision.radius : 3f;
        float covBias = Mathf.Max(0f, memCoverageBiasTexels) * texelWorld;

        Graphics.Blit(fogMemory, fogScratch);

        fogPainterMaterial.SetTexture(MainTexID, fogScratch);
        fogPainterMaterial.SetVector(WorldMinID, new Vector4(worldMin.x, worldMin.y, 0, 0));
        fogPainterMaterial.SetVector(WorldSizeID, new Vector4(size.x, size.y, 0, 0));
        fogPainterMaterial.SetVector(PlayerPosID, new Vector4(player.position.x, player.position.y, 0, 0));
        fogPainterMaterial.SetFloat(RadiusID, liveR);
        fogPainterMaterial.SetFloat(FalloffID, Mathf.Max(1e-6f, liveFalloff));
        fogPainterMaterial.SetFloat(MemWriteIntensityID, Mathf.Clamp01(memoryIntensity));
        fogPainterMaterial.SetFloat(MemCoverageBiasWorldID, covBias);
        fogPainterMaterial.SetInt(WriteLiveID, writeLiveToMemory ? 1 : 0);
        fogPainterMaterial.SetInt(WriteBurstsID, writeBurstsToMemory ? 1 : 0);
        fogPainterMaterial.SetInt(WriteQueuedID, writeQueuedToMemory ? 1 : 0);

        int burstCount = Mathf.Min(maxBursts, _bursts.Count);
        fogPainterMaterial.SetInt(BurstCountID, burstCount);
        if (burstCount > 0)
        {
            var posArray = new Vector4[burstCount];
            var radArray = new Vector4[burstCount];
            for (int i = 0; i < burstCount; i++)
            {
                var b = _bursts[i];
                posArray[i] = new Vector4(b.worldPos.x, b.worldPos.y, 0, 0);
                radArray[i] = new Vector4(b.radiusWorld, Mathf.Max(1e-6f, b.falloffWorld), 0, 0);
            }
            fogPainterMaterial.SetVectorArray(BurstPosID, posArray);
            fogPainterMaterial.SetVectorArray(BurstRadID, radArray);
        }

        int qCount = _queue.Count;
        fogPainterMaterial.SetInt(QueuedCountID, qCount);
        if (qCount > 0)
        {
            var qPos = new Vector4[qCount];
            var qRad = new Vector4[qCount];
            for (int i = 0; i < qCount; i++)
            {
                qPos[i] = new Vector4(_queue[i].worldPos.x, _queue[i].worldPos.y, 0, 0);
                qRad[i] = new Vector4(_queue[i].radiusWorld, 0f, _queue[i].intensity, _queue[i].mode == WriteMode.MAX ? 1f : 0f);
            }
            fogPainterMaterial.SetVectorArray(QueuedPosID, qPos);
            fogPainterMaterial.SetVectorArray(QueuedRadID, qRad);
        }

        Graphics.Blit(fogScratch, fogMemory, fogPainterMaterial);
        _queue.Clear();

        if (fogDisplayMaterial && playerVision)
        {
            fogDisplayMaterial.SetInt(DispBurstCountID, burstCount);
            if (burstCount > 0)
            {
                var posArray = new Vector4[burstCount];
                var radArray = new Vector4[burstCount];
                for (int i = 0; i < burstCount; i++)
                {
                    var b = _bursts[i];
                    posArray[i] = new Vector4(b.worldPos.x, b.worldPos.y, 0, 0);
                    radArray[i] = new Vector4(b.radiusWorld, Mathf.Max(1e-6f, b.falloffWorld), 0, 0);
                }
                fogDisplayMaterial.SetVectorArray(DispBurstPosID, posArray);
                fogDisplayMaterial.SetVectorArray(DispBurstRadID, radArray);
            }

            fogDisplayMaterial.SetVector(DispPlayerPosID, new Vector4(player.position.x, player.position.y, 0, 0));
            fogDisplayMaterial.SetFloat(DispRadiusID, liveR);
            fogDisplayMaterial.SetFloat(DispFalloffID, Mathf.Max(1e-6f, liveFalloff));
            fogDisplayMaterial.SetFloat(DispMemAlphaID, memoryAlpha);
        }
    }
}

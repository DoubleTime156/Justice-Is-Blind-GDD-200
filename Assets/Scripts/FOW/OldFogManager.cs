using System.Collections.Generic;
using UnityEngine;

public class FogManager : MonoBehaviour
{
    public PlayerPosition playerVision;           
    public Material fogPainterMaterial;          
    public Material fogDisplayMaterial;           
    public Transform player;

    public int rtSize = 2048;
    private RenderTexture fogMemory;
    private RenderTexture fogScratch;

    public Vector2 worldMin = new Vector2(-100f, -100f);
    public Vector2 worldMax = new Vector2(100f, 100f);

    public float liveFalloff = 0.5f;            
    private enum WriteMode { LERP = 0, MAX = 1 }

    private struct RevealReq
    {
        public Vector2 uv;
        public float radiusUV;
        public float intensity;
        public float edge;
        public WriteMode writeMode;
    }

    private readonly List<RevealReq> _queue = new();

    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int PositionID = Shader.PropertyToID("_Position");
    private static readonly int RadiusID = Shader.PropertyToID("_Radius");
    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int EdgeID = Shader.PropertyToID("_Edge");
    private static readonly int WriteModeID = Shader.PropertyToID("_WriteMode");

    public float defaultBurstFalloff = 0.5f;      
    public int maxBursts = 8;                   
    private struct Burst
    {
        public Vector2 worldPos;
        public float radiusWorld;
        public float falloffWorld;
        public float timer;                       
    }
    private readonly List<Burst> _bursts = new();

    private static readonly int BurstCountID = Shader.PropertyToID("_BurstCount");
    private static readonly int BurstPosID = Shader.PropertyToID("_BurstPos");
    private static readonly int BurstRadID = Shader.PropertyToID("_BurstRad");

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

        fogMemory.filterMode = FilterMode.Point;
        fogScratch.filterMode = FilterMode.Point;

        fogMemory.wrapMode = TextureWrapMode.Clamp;
        fogScratch.wrapMode = TextureWrapMode.Clamp;

        fogMemory.Create();
        fogScratch.Create();

        if (fogDisplayMaterial)
            fogDisplayMaterial.SetTexture("_FogTex", fogMemory);
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

    public void EnqueueReveal(Vector2 worldPos, float radiusUV, float intensity = 1f, float edge = 0.02f, bool brightenOnly = false)
    {
        Vector2 uv = WorldToUV(worldPos);
        _queue.Add(new RevealReq
        {
            uv = uv,
            radiusUV = radiusUV,
            intensity = intensity,
            edge = edge,
            writeMode = brightenOnly ? WriteMode.MAX : WriteMode.LERP
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
            falloffWorld = Mathf.Max(0.0001f, falloffWorld),
            timer = Mathf.Max(0f, durationSeconds)
        });
    }

    public void FlushNow() => LateUpdate();

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
        if (player == null)
        {
            _queue.Clear();
            return;
        }

        if (fogDisplayMaterial && playerVision)
        {
            for (int i = _bursts.Count - 1; i >= 0; --i)
            {
                Burst b = _bursts[i];
                b.timer -= Time.deltaTime;
                if (b.timer <= 0f) _bursts.RemoveAt(i);
                else _bursts[i] = b;
            }

            int burstCount = Mathf.Min(maxBursts, 1 + _bursts.Count);
            Vector4[] posArray = new Vector4[Mathf.Max(burstCount, 1)];
            Vector4[] radArray = new Vector4[Mathf.Max(burstCount, 1)]; 

            posArray[0] = new Vector4(player.position.x, player.position.y, 0, 0);
            radArray[0] = new Vector4(playerVision.radius, liveFalloff, 0, 0);

            for (int i = 0; i < burstCount - 1; i++)
            {
                var b = _bursts[i];
                posArray[i + 1] = new Vector4(b.worldPos.x, b.worldPos.y, 0, 0);
                radArray[i + 1] = new Vector4(b.radiusWorld, b.falloffWorld, 0, 0);
            }

            fogDisplayMaterial.SetInt(BurstCountID, burstCount);
            fogDisplayMaterial.SetVectorArray(BurstPosID, posArray);
            fogDisplayMaterial.SetVectorArray(BurstRadID, radArray);

            fogDisplayMaterial.SetVector("_PlayerPos", new Vector4(player.position.x, player.position.y, 0, 0));
            fogDisplayMaterial.SetFloat("_Radius", playerVision.radius);
            fogDisplayMaterial.SetFloat("_Falloff", liveFalloff);
        }

        if (fogPainterMaterial && playerVision)
        {
            Vector2 worldSize = worldMax - worldMin;
            float radiusUV = playerVision.radius / Mathf.Min(worldSize.x, worldSize.y);
            Vector2 uv = WorldToUV(player.position);

            _queue.Add(new RevealReq
            {
                uv = uv,
                radiusUV = radiusUV,
                intensity = 0.3f,   
                edge = 0.02f,
                writeMode = WriteMode.MAX 
            });
        }

        if (_queue.Count > 0 && fogPainterMaterial != null)
        {
            Graphics.Blit(fogMemory, fogScratch);

            for (int i = 0; i < _queue.Count; i++)
            {
                var r = _queue[i];
                fogPainterMaterial.SetVector(PositionID, new Vector4(r.uv.x, r.uv.y, 0, 0));
                fogPainterMaterial.SetFloat(RadiusID, r.radiusUV);
                fogPainterMaterial.SetFloat(IntensityID, r.intensity);
                fogPainterMaterial.SetFloat(EdgeID, r.edge);
                fogPainterMaterial.SetFloat(WriteModeID, (r.writeMode == WriteMode.MAX) ? 1f : 0f);
                fogPainterMaterial.SetTexture(MainTexID, fogScratch);

                Graphics.Blit(fogScratch, fogMemory, fogPainterMaterial);
                Graphics.Blit(fogMemory, fogScratch);
            }
        }

        _queue.Clear();
    }
}

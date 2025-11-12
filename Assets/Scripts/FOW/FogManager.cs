using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
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

    public float liveFalloff = 0.75f;
    public float memoryAlpha = 0.35f;
    public float memoryEdgeWorld = 0.6f;
    public float memoryIntensity = 0.3f;

    public float memoryShrinkTexels = 1.25f;

    private enum WriteMode { LERP = 0, MAX = 1 }

    private struct RevealReq
    {
        public Vector2 worldPos;
        public float radiusWorld;
        public float intensity;
        public float edgeWorld;
        public WriteMode writeMode;
    }

    private readonly List<RevealReq> _queue = new();

    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int WorldMinID = Shader.PropertyToID("_WorldMin");
    private static readonly int WorldSizeID = Shader.PropertyToID("_WorldSize");
    private static readonly int PosWorldID = Shader.PropertyToID("_PositionWorld");
    private static readonly int RadiusWID = Shader.PropertyToID("_RadiusWorld");
    private static readonly int EdgeWID = Shader.PropertyToID("_EdgeWorld");
    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int WriteModeID = Shader.PropertyToID("_WriteMode");

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
        var desc = new RenderTextureDescriptor(rtSize, rtSize, RenderTextureFormat.ARGB32, 0)
        { useMipMap = true, autoGenerateMips = true, msaaSamples = 1 };
        fogMemory = new RenderTexture(desc);
        fogScratch = new RenderTexture(desc);

        fogMemory.filterMode = FilterMode.Bilinear;
        fogScratch.filterMode = FilterMode.Bilinear;
        fogMemory.wrapMode = TextureWrapMode.Clamp;
        fogScratch.wrapMode = TextureWrapMode.Clamp;
        fogMemory.anisoLevel = 8;
        fogScratch.anisoLevel = 8;

        fogMemory.Create();
        fogScratch.Create();

        if (fogDisplayMaterial) fogDisplayMaterial.SetTexture("_FogTex", fogMemory);
    }

    void PushWorldParamsToMaterials()
    {
        Vector2 worldSize = worldMax - worldMin;
        Vector4 minV = new Vector4(worldMin.x, worldMin.y, 0, 0);
        Vector4 sizeV = new Vector4(worldSize.x, worldSize.y, 0, 0);

        if (fogPainterMaterial)
        {
            fogPainterMaterial.SetVector(WorldMinID, minV);
            fogPainterMaterial.SetVector(WorldSizeID, sizeV);
        }
        if (fogDisplayMaterial)
        {
            fogDisplayMaterial.SetVector(WorldMinID, minV);
            fogDisplayMaterial.SetVector(WorldSizeID, sizeV);
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

    public void EnqueueRevealWorld(Vector2 worldPos, float radiusWorld, float intensity = 1f, float edgeWorld = 0.5f, bool brightenOnly = false)
    {
        _queue.Add(new RevealReq
        {
            worldPos = worldPos,
            radiusWorld = Mathf.Max(0f, radiusWorld),
            intensity = intensity,
            edgeWorld = Mathf.Max(0f, edgeWorld),
            writeMode = brightenOnly ? WriteMode.MAX : WriteMode.LERP
        });
    }

    public void FlushNow() => LateUpdate();

    void LateUpdate()
    {
        if (player == null)
        {
            _queue.Clear();
            return;
        }

        Vector2 worldSize = worldMax - worldMin;
        float minWorld = Mathf.Min(worldSize.x, worldSize.y);
        float texelWorld = minWorld / Mathf.Max(1, rtSize);

        float safeEdge = Mathf.Max(memoryEdgeWorld, 1.5f * texelWorld);
        float shrinkW = Mathf.Max(0f, memoryShrinkTexels) * texelWorld;

        float liveR = playerVision ? playerVision.radius : 3f;
        float paintR = Mathf.Max(0f, liveR - shrinkW);

        EnqueueRevealWorld((Vector2)player.position, paintR, memoryIntensity, safeEdge, true);

        if (_queue.Count > 0 && fogPainterMaterial != null)
        {
            fogPainterMaterial.SetVector(WorldMinID, new Vector4(worldMin.x, worldMin.y, 0, 0));
            fogPainterMaterial.SetVector(WorldSizeID, new Vector4(worldSize.x, worldSize.y, 0, 0));

            Graphics.Blit(fogMemory, fogScratch);

            for (int i = 0; i < _queue.Count; i++)
            {
                var r = _queue[i];
                fogPainterMaterial.SetVector(PosWorldID, new Vector4(r.worldPos.x, r.worldPos.y, 0, 0));
                fogPainterMaterial.SetFloat(RadiusWID, r.radiusWorld);
                fogPainterMaterial.SetFloat(EdgeWID, Mathf.Max(1e-6f, r.edgeWorld));
                fogPainterMaterial.SetFloat(IntensityID, r.intensity);
                fogPainterMaterial.SetFloat(WriteModeID, (r.writeMode == WriteMode.MAX) ? 1f : 0f);
                fogPainterMaterial.SetTexture(MainTexID, fogScratch);

                Graphics.Blit(fogScratch, fogMemory, fogPainterMaterial);
                Graphics.Blit(fogMemory, fogScratch);
            }
        }
        _queue.Clear();

        if (fogDisplayMaterial && playerVision)
        {
            fogDisplayMaterial.SetVector("_PlayerPos", new Vector4(player.position.x, player.position.y, 0, 0));
            fogDisplayMaterial.SetFloat("_Radius", liveR);
            fogDisplayMaterial.SetFloat("_Falloff", Mathf.Max(1e-6f, liveFalloff));
            fogDisplayMaterial.SetFloat("_MemoryAlpha", memoryAlpha);

            // NEW: tell the display shader how much to hide just outside the live circle
            fogDisplayMaterial.SetFloat("_MemInsetWorld", shrinkW);
        }
    }
}

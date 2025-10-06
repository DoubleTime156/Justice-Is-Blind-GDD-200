using UnityEngine;

[ExecuteAlways]
public class FogManager : MonoBehaviour
{
    [Header("References")]
    public Material fogPainterMaterial;  
    public Material fogDisplayMaterial;   
    public Transform player;               

    [Header("Fog RT")]
    public int rtSize = 512;
    private RenderTexture fogMemory;

    [Header("World mapping (set to your map bounds)")]
    public Vector2 worldMin = new Vector2(-10f, -10f);
    public Vector2 worldMax = new Vector2(10f, 10f);

    [Header("Paint settings (UV space)")]
    [Range(0.01f, 1f)]
    public float revealRadiusUV = 0.05f; 


    private static readonly int PositionID = Shader.PropertyToID("_Position");
    private static readonly int RadiusID = Shader.PropertyToID("_Radius");

    void Start()
    {
        InitializeRenderTexture();
        PushWorldParamsToMaterials();
    }

    void OnValidate()
    {
      
        if (Application.isPlaying) return;
        InitializeRenderTexture();
        PushWorldParamsToMaterials();
    }

    void InitializeRenderTexture()
    {
        if (fogMemory != null)
        {
            fogMemory.Release();
            DestroyImmediate(fogMemory);
        }

        fogMemory = new RenderTexture(rtSize, rtSize, 0, RenderTextureFormat.ARGB32);
        fogMemory.filterMode = FilterMode.Bilinear;
        fogMemory.wrapMode = TextureWrapMode.Clamp;
        fogMemory.Create();

        var prev = RenderTexture.active;
        RenderTexture.active = fogMemory;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;

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
            fogDisplayMaterial.SetTexture("_FogTex", fogMemory);
        }
    }

    void Update()
    {
        if (player == null || fogPainterMaterial == null || fogMemory == null) return;

        Vector2 worldPos = new Vector2(player.position.x, player.position.y);
        Vector2 worldSize = worldMax - worldMin;
        Vector2 uv = (worldPos - worldMin);
        uv.x = worldSize.x != 0 ? uv.x / worldSize.x : 0f;
        uv.y = worldSize.y != 0 ? uv.y / worldSize.y : 0f;

        uv = new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));

        fogPainterMaterial.SetVector(PositionID, new Vector4(uv.x, uv.y, 0, 0));
        fogPainterMaterial.SetFloat(RadiusID, revealRadiusUV);

        RenderTexture temp = RenderTexture.GetTemporary(fogMemory.width, fogMemory.height, 0, fogMemory.format);
        Graphics.Blit(fogMemory, temp);                    
        Graphics.Blit(temp, fogMemory, fogPainterMaterial);
        RenderTexture.ReleaseTemporary(temp);
    }
}

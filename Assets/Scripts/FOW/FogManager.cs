using UnityEngine;

[ExecuteAlways]
public class FogManager : MonoBehaviour
{
    //material refrences
    public PlayerPosition playerVision;
    public Material fogPainterMaterial;  
    public Material fogDisplayMaterial;   
    public Transform player;               

    //fog rt and memory
    public int rtSize = 1024;
    private RenderTexture fogMemory;

    //world bounds, if map is bigger then this vector vision will glitch out
    public Vector2 worldMin = new Vector2(-100f, -100f);
    public Vector2 worldMax = new Vector2(100f, 100f);

    //revela radius in UV
    [Range(0.01f, 1f)]
    public float revealRadiusUV = 0.25f;

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
            //sets the paint to where it needs to be in the world
            fogPainterMaterial.SetVector("_WorldMin", new Vector4(worldMin.x, worldMin.y, 0, 0));
            fogPainterMaterial.SetVector("_WorldSize", new Vector4(worldSize.x, worldSize.y, 0, 0));
        }
        if (fogDisplayMaterial)
        {
            //sets the paint to where it needs to be in the world
            fogDisplayMaterial.SetVector("_WorldMin", new Vector4(worldMin.x, worldMin.y, 0, 0));
            fogDisplayMaterial.SetVector("_WorldSize", new Vector4(worldSize.x, worldSize.y, 0, 0));
            fogDisplayMaterial.SetTexture("_FogTex", fogMemory);
        }
    }
    public RenderTexture GetFogMemory()
    {
        return fogMemory;
    }
    void Update()
    {
        if (player == null || fogPainterMaterial == null) return;

        // Get world size and position
        Vector2 worldPos = new Vector2(player.position.x, player.position.y);
        Vector2 worldSize = worldMax - worldMin;

        // Convert player world position to UV coordinates (0–1)
        Vector2 uv = (worldPos - worldMin);
        uv.x = worldSize.x != 0 ? uv.x / worldSize.x : 0f;
        uv.y = worldSize.y != 0 ? uv.y / worldSize.y : 0f;
        uv = new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));

        // Convert the player's world radius to UV radius
        float radiusUV = revealRadiusUV; 
        if (playerVision != null)
        {
            // Convert from world-space radius to UV-space radius
            float radiusX = playerVision.radius / worldSize.x;
            float radiusY = playerVision.radius / worldSize.y;
            radiusUV = Mathf.Sqrt(radiusX * radiusY);
        }

        // Update the fog shader
        fogPainterMaterial.SetVector("_Position", new Vector4(uv.x, uv.y, 0, 0));
        fogPainterMaterial.SetFloat("_Radius", radiusUV);

        // Paint the vision area into the fog memory texture
        RenderTexture temp = RenderTexture.GetTemporary(fogMemory.width, fogMemory.height, 0, fogMemory.format);
        Graphics.Blit(fogMemory, temp);
        Graphics.Blit(temp, fogMemory, fogPainterMaterial);

        radiusUV = revealRadiusUV;
        if (playerVision != null)
        {
            radiusUV = playerVision.radius / worldSize.x;   

            float correction = 0.93f;   
            radiusUV *= correction;
        }
    }

}

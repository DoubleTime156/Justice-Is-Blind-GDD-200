using UnityEngine;

[ExecuteAlways]
public class FogManager : MonoBehaviour
{
    //material refrences
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

    void Update()
    {
        //player pos to world pos
        Vector2 worldPos = new Vector2(player.position.x, player.position.y);
        //sets the shaders to cover world size
        Vector2 worldSize = worldMax - worldMin;
        // gets player pos in given world size
        Vector2 uv = (worldPos - worldMin);
        uv.x = worldSize.x != 0 ? uv.x / worldSize.x : 0f;
        uv.y = worldSize.y != 0 ? uv.y / worldSize.y : 0f;

        uv = new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));

        //puts material where player is
        fogPainterMaterial.SetVector(PositionID, new Vector4(uv.x, uv.y, 0, 0));
        fogPainterMaterial.SetFloat(RadiusID, revealRadiusUV);


        RenderTexture temp = RenderTexture.GetTemporary(fogMemory.width, fogMemory.height, 0, fogMemory.format);
        Graphics.Blit(fogMemory, temp);                    
        Graphics.Blit(temp, fogMemory, fogPainterMaterial);
        RenderTexture.ReleaseTemporary(temp);
    }
}

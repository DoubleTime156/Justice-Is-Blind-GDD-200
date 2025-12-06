using UnityEngine;

public class RendererSetup : MonoBehaviour
{
    public FogManager fog;
    public Camera unlitCamera;
    public int pixelsPerUnit = 100;   

    private RenderTexture rt;

    void OnEnable()
    {
        Rebuild();
    }

    void OnDisable()
    {
        if (rt != null)
        {
            if (unlitCamera != null && unlitCamera.targetTexture == rt)
                unlitCamera.targetTexture = null;

            rt.Release();
#if UNITY_EDITOR
            DestroyImmediate(rt);
#else
            Destroy(rt);
#endif
        }
    }

    public void Rebuild()
    {
        if (fog == null || unlitCamera == null) return;

        Vector2 size = fog.worldMax - fog.worldMin;
        float worldWidth = Mathf.Abs(size.x);
        float worldHeight = Mathf.Abs(size.y);

        int width = Mathf.RoundToInt(worldWidth * pixelsPerUnit);
        int height = Mathf.RoundToInt(worldHeight * pixelsPerUnit);



        if (rt != null)
        {
            rt.Release();
#if UNITY_EDITOR
            DestroyImmediate(rt);
#else
            Destroy(rt);
#endif
        }

        var desc = new RenderTextureDescriptor(width, height,
                                               RenderTextureFormat.ARGB32,
                                               0);
        desc.useMipMap = false;
        desc.autoGenerateMips = false;
        desc.msaaSamples = 1;

        rt = new RenderTexture(desc);
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.filterMode = FilterMode.Point;
        rt.Create();

        unlitCamera.targetTexture = rt;
        fog.unlitWorldTexture = rt;


    }

}

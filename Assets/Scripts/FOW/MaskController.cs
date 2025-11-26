using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-90)]
public class MaskController : MonoBehaviour
{
    public FogManager fogManager;
    public LayerMask maskLayer;
    public int rtSize = 4096;
    public int rendererIndex = 0;

    public RenderTexture LiveMaskRT => liveMaskRT;

    Camera cam;
    RenderTexture liveMaskRT;

    void Awake()
    {
        if (fogManager == null) fogManager = FindObjectOfType<FogManager>();
        CreateOrResizeRT();
        CreateOrSetupCamera();
    }

    void OnDestroy()
    {
        if (liveMaskRT) { liveMaskRT.Release(); Destroy(liveMaskRT); }
        if (cam) Destroy(cam.gameObject);
    }

    void LateUpdate()
    {
        if (fogManager == null) return;

        if (liveMaskRT == null || liveMaskRT.width != rtSize || liveMaskRT.height != rtSize)
            CreateOrResizeRT();

        if (cam == null) CreateOrSetupCamera();

        var min = fogManager.worldMin;
        var max = fogManager.worldMax;
        var size = max - min;
        var center = (min + max) * 0.5f;

        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(0.0001f, size.y * 0.5f);
        cam.transform.position = new Vector3(center.x, center.y, -10f);
        cam.aspect = Mathf.Max(0.0001f, size.x / Mathf.Max(0.0001f, size.y));
        cam.cullingMask = maskLayer;
        cam.targetTexture = liveMaskRT;
    }

    void CreateOrResizeRT()
    {
        if (liveMaskRT)
        {
            liveMaskRT.Release();
            Destroy(liveMaskRT);
        }

        var desc = new RenderTextureDescriptor(rtSize, rtSize, RenderTextureFormat.ARGB32, 0);
        desc.msaaSamples = 1;
        desc.useMipMap = false;
        desc.autoGenerateMips = false;
        desc.sRGB = false;

        liveMaskRT = new RenderTexture(desc);
        liveMaskRT.name = "LiveMaskRT";
        liveMaskRT.wrapMode = TextureWrapMode.Clamp;
        liveMaskRT.filterMode = FilterMode.Bilinear;
        liveMaskRT.anisoLevel = 0;
        liveMaskRT.Create();
    }

    void CreateOrSetupCamera()
    {
        if (cam == null)
        {
            var go = new GameObject("FogMaskCam");
            go.hideFlags = HideFlags.DontSave;
            cam = go.AddComponent<Camera>();
            var uacd = go.GetComponent<UniversalAdditionalCameraData>();
            if (uacd == null) uacd = go.AddComponent<UniversalAdditionalCameraData>();
            uacd.renderType = CameraRenderType.Base;
            uacd.SetRenderer(rendererIndex);
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.allowHDR = false;
        cam.allowMSAA = false;
        cam.depth = -100;
    }
}

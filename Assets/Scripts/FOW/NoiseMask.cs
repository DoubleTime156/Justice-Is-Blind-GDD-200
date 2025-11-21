// NoiseMask.cs
using UnityEngine;

public class NoiseMask : MonoBehaviour
{
    public Material maskMaterial;
    public string fogMaskLayerName = "FogMask";
    public LayerMask obstacleMask;
    public float radiusWorld = 6f;
    public float holdSeconds = 0.8f;
    public int rayCount = 512;

    Mesh mesh;
    float timer;

    public void Init(Material mat, string fogLayer, LayerMask obstacle, Vector2 worldPos, float radius, float hold, int rays)
    {
        maskMaterial = mat;
        fogMaskLayerName = fogLayer;
        obstacleMask = obstacle;
        transform.position = worldPos;
        radiusWorld = Mathf.Max(0f, radius);
        holdSeconds = Mathf.Max(0.0001f, hold);
        rayCount = Mathf.Clamp(rays, 32, 2048);
        BuildOnce();
    }

    void BuildOnce()
    {
        gameObject.layer = LayerMask.NameToLayer(fogMaskLayerName);

        var mf = GetComponent<MeshFilter>();
        if (!mf) mf = gameObject.AddComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();
        if (!mr) mr = gameObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = maskMaterial;

        if (mesh == null) mesh = new Mesh();
        mesh.name = "NoiseMaskMesh";
        mf.sharedMesh = mesh;

        Vector3[] verts = new Vector3[rayCount + 1];
        int[] tris = new int[rayCount * 3];
        verts[0] = Vector3.zero;

        float step = 360f / rayCount;
        for (int i = 0; i < rayCount; i++)
        {
            float ang = step * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

            var hit = Physics2D.Raycast((Vector2)transform.position, dir, radiusWorld, obstacleMask);
            Vector2 end = hit ? (Vector2)transform.position + dir * (hit.distance - 0.01f)
                              : (Vector2)transform.position + dir * radiusWorld;

            verts[i + 1] = transform.InverseTransformPoint(end);

            int a = 0, b = i + 1, c = (i + 1) % rayCount + 1;
            int t = i * 3;
            tris[t] = a; tris[t + 1] = c; tris[t + 2] = b;
        }

        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;

        timer = holdSeconds;
        enabled = true;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) Destroy(gameObject);
    }

    public static void Spawn(Material mat, string fogLayer, LayerMask obstacle, Vector2 pos, float radius, float hold, int rays)
    {
        var go = new GameObject("NoiseMask");
        var burst = go.AddComponent<NoiseMask>();
        burst.Init(mat, fogLayer, obstacle, pos, radius, hold, rays);
    }
}

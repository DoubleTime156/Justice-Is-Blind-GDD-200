using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HideEnemies : MonoBehaviour
{
    public VisionRaycast vision;        // assign in Inspector (the same object that builds the raycast mesh)
    public string enemyTag = "Enemy";   // parent "Goon" should have this tag

    MeshFilter visionMF;

    void Awake()
    {
        if (!vision) vision = FindObjectOfType<VisionRaycast>();
        if (vision) visionMF = vision.GetComponent<MeshFilter>();
    }

    void LateUpdate()
    {
        if (!vision || !visionMF) return;

        var mesh = visionMF.sharedMesh;
        if (!mesh || mesh.vertexCount < 3) return;

        var verts = mesh.vertices;      // local to VisionRaycast
        var tris = mesh.triangles;     // triangle fan: [0, i+1, next]

        var enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        for (int i = 0; i < enemies.Length; i++)
        {
            var go = enemies[i];
            if (!go) continue;

            // Preferred: centralized visibility controller
            var ev = go.GetComponent<EnemyParent>();

            // Sample point (center of first renderer bounds if available, else transform)
            Vector3 sample = (ev != null) ? ev.SamplePoint() : go.transform.position;

            // Convert sample into VisionRaycast local space and test against the mesh
            Vector2 pLocal = vision.transform.InverseTransformPoint(sample);
            bool inside = PointInMesh(pLocal, verts, tris);

            if (ev != null)
            {
                ev.SetVisible(inside);
            }
            else
            {
                // Fallback: toggle all child renderers and Light2D
                var rends = go.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < rends.Length; r++) if (rends[r]) rends[r].enabled = inside;

                var lights = go.GetComponentsInChildren<Light2D>(true);
                for (int l = 0; l < lights.Length; l++) if (lights[l]) lights[l].enabled = inside;
            }
        }
    }

    static bool PointInMesh(Vector2 p, Vector3[] verts, int[] tris)
    {
        // Mesh is a fan: test point against each triangle in local space
        for (int i = 0; i + 2 < tris.Length; i += 3)
        {
            Vector2 a = (Vector2)verts[tris[i + 0]];
            Vector2 b = (Vector2)verts[tris[i + 1]];
            Vector2 c = (Vector2)verts[tris[i + 2]];
            if (PointInTriangleInclusive(p, a, b, c)) return true;
        }
        return false;
    }

    static bool PointInTriangleInclusive(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        // Barycentric sign test, inclusive on edges
        float s1 = Cross(b - a, p - a);
        float s2 = Cross(c - b, p - b);
        float s3 = Cross(a - c, p - c);

        bool hasNeg = (s1 < 0f) || (s2 < 0f) || (s3 < 0f);
        bool hasPos = (s1 > 0f) || (s2 > 0f) || (s3 > 0f);

        // inside if all have same sign or zero (on the edge)
        return !(hasNeg && hasPos);
    }

    static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;
}

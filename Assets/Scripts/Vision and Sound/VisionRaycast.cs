using System.Collections.Generic;
using UnityEngine;

public class VisionRaycast : MonoBehaviour
{
    public float visionRadius;

    private int _rayCount = 360;
    int pointCount;

    private int _pointCount;
    private List<Vector2> _pointPos;

    struct RaySample
    {
        public Vector2 pos;
        public Vector2 dir;
    }

    RaySample[] rays;
    Mesh mesh;
    bool active;
    float t;

    float _deathTime = 0.2f;

    void Awake()
    {
        active = true;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        rays = new RaySample[_rayCount];
        _pointPos = new List<Vector2>();

        Trigger();
    }

    void FixedUpdate()
    {
        Trigger();
        UpdateRays();
        BuildMesh();
    }

    void Trigger()
    {
        active = true;
        t = 0f;
        _pointPos.Clear();
        _pointCount = _rayCount;

        float step = 360f / _rayCount;
        for (int i = 0; i < _rayCount; i++)
        {
            float ang = step * i * Mathf.Deg2Rad;
            rays[i].pos = transform.position;
            rays[i].dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

            _pointPos.Add(rays[i].pos);
        }
    }

    void UpdateRays()
    {
        for (int i = 0; i < _rayCount; i++)
        {

            RaycastHit2D hit = Physics2D.Raycast(rays[i].pos, rays[i].dir, visionRadius, LayerMask.GetMask("Obstacle"));
            if (hit)
            {
                // Go to wall if hit
                rays[i].pos += rays[i].dir * (hit.distance - 0.01f);
            }
            else
            {
                // Go to vision radius if no hit
                rays[i].pos += rays[i].dir * visionRadius;
            }

            _pointPos[i] = rays[i].pos;
        }
    }

    void BuildMesh()
    {
        // Local mesh vertices
        Vector3[] vertices = new Vector3[_pointCount + 1];
        vertices[0] = Vector3.zero;

        for (int i = 0; i < _pointCount; i++)
        {
            // Convert world point to local point
            vertices[i + 1] = transform.InverseTransformPoint(_pointPos[i]);
        }

        // Create triangles for mesh
        int[] tris = new int[_pointCount * 3];
        for (int i = 0; i < _pointCount; i++)
        {
            int a = 0;
            int b = i + 1;
            int c = (i + 1) % _pointCount + 1;
            int ti = i * 3;
            tris[ti] = a;
            tris[ti + 1] = c;
            tris[ti + 2] = b;
        }

        // Build mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = tris;
    }

    public bool IsPointInsideVision(Vector2 worldPoint)
    {
        if (mesh == null || mesh.vertexCount < 3) return false;

        var verts = mesh.vertices;
        int n = verts.Length - 1;
        if (n < 3) return false;

        Vector2 p = worldPoint;
        Vector2 prev = (Vector2)transform.TransformPoint(verts[n]);
        bool inside = false;

        for (int i = 1; i <= n; i++)
        {
            Vector2 cur = (Vector2)transform.TransformPoint(verts[i]);
            bool cond = ((cur.y > p.y) != (prev.y > p.y));
            if (cond)
            {
                float xInt = (prev.x - cur.x) * (p.y - cur.y) / ((prev.y - cur.y) + Mathf.Epsilon) + cur.x;
                if (p.x < xInt) inside = !inside;
            }
            prev = cur;
        }
        return inside;
    }
}

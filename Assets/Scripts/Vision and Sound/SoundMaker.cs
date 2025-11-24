using System;
using System.Collections.Generic;
using System.Numerics;
using Mono.Cecil.Cil;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class SoundMaker : MonoBehaviour
{
    public bool IsMakingSound { get; set; }
    public float soundRadius;

    private int _rayCount = 360;
    private float _speed = 15.0f;
    private float _lifetime;

    private int _pointCount;
    private List<Vector2> _pointPos;

    struct RaySample
    {
        public Vector2 pos;
        public Vector2 dir;
        public float life;
    }

    RaySample[] rays;
    Mesh mesh;
    PolygonCollider2D poly;
    bool active;
    float t;

    float _deathTime = 0.2f;

    void Awake()
    {
        IsMakingSound = false;
        active = true;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        poly = GetComponent<PolygonCollider2D>();
        rays = new RaySample[_rayCount];
        _pointPos = new List<Vector2>();
        _lifetime = soundRadius / _speed;
        _deathTime += _lifetime;

        Trigger();
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        t += dt;

        UpdateRays(dt);
        BuildMeshAndCollider();

        // Start countdown to death after t > _lifetime
        if (t >= _lifetime) active = false;
        else return;

        if (t >= _deathTime) Destroy(gameObject);
    }

    void Trigger()
    {
        active = true;
        t = 0f;
        _pointPos.Clear();
        _pointCount = _rayCount;
        _hitPoints = 0;

        float step = 360f / _rayCount;
        for (int i = 0; i < _rayCount; i++)
        {
            float ang = step * i * Mathf.Deg2Rad;
            rays[i].pos = transform.position;
            rays[i].dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            rays[i].life = _lifetime;

            _pointPos.Add(rays[i].pos);
        }
    }

    private int _hitPoints;

    void UpdateRays(float dt)
    {
        for (int i = 0; i < _rayCount; i++)
        {
            // if life = 0, skip
            if (rays[i].life <= 0) continue;

            RaycastHit2D hit = Physics2D.Raycast(rays[i].pos, rays[i].dir, _speed * dt, LayerMask.GetMask("Obstacle"));
            if (hit)
            {
                // Stop moving if hit
                rays[i].life = 0;
                rays[i].pos += rays[i].dir * (hit.distance - 0.01f);
            }
            else
            {
                // Movement
                rays[i].pos += rays[i].dir * _speed * dt;
                //rays[i].life -= dt;
            }


            _pointPos[i] = rays[i].pos;
        }
    }

    void BuildMeshAndCollider()
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

        Color[] colors = new Color[vertices.Length];

        // Find max distance (edge) for gradient scaling
        float maxDist = 0f;
        for (int i = 1; i < vertices.Length; i++)
            maxDist = Mathf.Max(maxDist, vertices[i].magnitude);

        // Center vertex transparent
        colors[0] = new Color(1f, 1f, 1f, 0f);

        // Bright edge
        for (int i = 1; i < vertices.Length; i++)
        {
            float alpha = vertices[i].magnitude / maxDist;
            alpha *= (_deathTime - t);
            alpha = Mathf.Clamp01(alpha);

            colors[i] = new Color(1f, 1f, 1f, alpha);
        }

        // Build mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.colors = colors;

        // Only build visual if not active, do not build collider
        if (!active) return;

        // Create collider
        Vector2[] pts = new Vector2[_pointCount];
        for (int i = 0; i < _pointCount; i++)
            pts[i] = vertices[i + 1];

        poly.SetPath(0, pts);
    }

    // Debug - Sound Circle
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }
}
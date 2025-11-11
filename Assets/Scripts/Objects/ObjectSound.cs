using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ObjectSound : MonoBehaviour
{
    public bool IsMakingSound { get; set; }
    public float soundRadius;

    private int _rayCount = 360;
    private float _speed = 10.0f;
    private float _lifetime = 0.7f;

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

    void Awake()
    {
        IsMakingSound = false;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        poly = GetComponent<PolygonCollider2D>();
        rays = new RaySample[_rayCount];
        _pointPos = new List<Vector2>();
        _lifetime = soundRadius / _speed;
    }

    private float myTimer = 0f;

    void FixedUpdate()
    {
        myTimer += Time.fixedDeltaTime;
        if (myTimer > 5f)
        {
            IsMakingSound = true;
            myTimer = 0;
        }

        if (IsMakingSound)
        {
            IsMakingSound = false;
            Trigger();
        }

        if (!active) return;

        float dt = Time.fixedDeltaTime;
        t += dt;

        UpdateRays(dt);
        BuildMeshAndCollider();

        if (t >= _lifetime) active = false;
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
            // move
            if (rays[i].life <= 0) continue;

            rays[i].pos += rays[i].dir * _speed * dt;
            rays[i].life -= dt;

            // reflect on colliders
            
            RaycastHit2D hit = Physics2D.Raycast(rays[i].pos, rays[i].dir, _speed * dt,
                LayerMask.GetMask("Obstacle"));
            if (hit)
            {
                rays[i].life = 0;
                rays[i].pos = hit.point;
                rays[i].dir = Vector2.Reflect(rays[i].dir, hit.normal).normalized;
                /*
                _pointPos.Insert(i + _hitPoints, rays[i].pos);
                _pointCount++;
                _hitPoints++;
                */
                
            }
            
            _pointPos[i] = rays[i].pos;
        }
    }

    void BuildMeshAndCollider()
    {
        Vector3[] verts = new Vector3[_pointCount + 1];
        verts[0] = Vector3.zero; // center at the object's origin in local space

        for (int i = 0; i < _pointCount; i++)
        {
            // convert world point to local space of this transform
            verts[i + 1] = transform.InverseTransformPoint(_pointPos[i]);
        }

        // triangles
        int[] tris = new int[_pointCount * 3];
        for (int i = 0; i < _pointCount; i++)
        {
            int a = 0;
            int b = i + 1;
            int c = (i + 1) % _pointCount + 1;
            int ti = i * 3;
            tris[ti] = a;
            tris[ti + 1] = b;
            tris[ti + 2] = c;
        }

        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;


        // collider path (exclude center vertex)
        Vector2[] pts = new Vector2[_pointCount];
        for (int i = 0; i < _pointCount; i++)
            pts[i] = verts[i + 1];

        poly.SetPath(0, pts);
    }

    // Debug - Sound Circle
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }
}
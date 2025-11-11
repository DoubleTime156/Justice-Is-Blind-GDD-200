using System;
using UnityEngine;

public class SoundMeter : MonoBehaviour
{
    public int resolution = 100;
    public float amplitude = 1f;
    public float length = Mathf.PI*2;
    
    [SerializeField] private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        DrawWave();
    }

    void DrawWave()
    {
        lineRenderer.positionCount = resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1);
            float x = t * length - length/2;
            float y = SoundFunction(x);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    float SoundFunction(float x)
    {
        return amplitude * (1 - Mathf.Pow(x, 2) / Mathf.Pow(Mathf.PI, 2)) *
               (0.5f + Mathf.Cos(x - Time.time - amplitude) / 2f) *
               Mathf.Sin((5f + 5f * amplitude) * x);
    }
}

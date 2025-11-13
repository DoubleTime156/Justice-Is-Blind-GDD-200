using System;
using UnityEngine;

public class SoundMeter : MonoBehaviour
{
    public int resolution = 100;
    public Transform SoundMeterTransform;
    public float soundLevel = 0.1f; // Sound from 0 to 1

    private float amplitude;
    private float length;

    [SerializeField] private LineRenderer lineRenderer;

    void Start()
    {
        length = SoundMeterTransform.localScale.x;
        amplitude = length / 2.0f;
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        length = SoundMeterTransform.localScale.x;
        amplitude = soundLevel * length / 2.0f;
        DrawWave();
    }

    void DrawWave()
    {
        lineRenderer.positionCount = resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1);
            float x = t * Mathf.PI*2.0f - Mathf.PI;
            float y = SoundFunction(x);
            Vector3 position = new Vector3(x * length / (MathF.PI * 2.0f), y, 0) + SoundMeterTransform.position;


            lineRenderer.SetPosition(i, position);
        }
    }

    float SoundFunction(float x)
    {
        return amplitude * (1.0f - Mathf.Pow(x / Mathf.PI, 2.0f)) *
               (0.5f + MathF.Pow(Mathf.Cos(x - Time.time * 4.0f - amplitude), 2.0f) / 2.0f) *
               Mathf.Sin((5.0f + 5.0f * amplitude) * x);
    }
}

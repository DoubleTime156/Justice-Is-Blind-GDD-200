using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class OldSoundMeter : MonoBehaviour
{
    public int resolution;
    public float soundLevel = 0.1f; // Sound from 0 to 1

    private float amplitude;
    private float _scalar;

    public Transform uiParent;
    [SerializeField] private LineRenderer lineRenderer;

    void Start()
    {
        _scalar = uiParent.localScale.x;
        amplitude = _scalar / 2.0f;
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        _scalar = uiParent.localScale.x;
        amplitude = soundLevel * MathF.PI;
        DrawWave();
    }

    void DrawWave()
    {
        lineRenderer.positionCount = resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1); 
            float x = t * Mathf.PI * 2.0f - Mathf.PI; 
            float y = SoundFunction(x) * 0.5f * _scalar / MathF.PI; 
            Vector3 position = new Vector3(x * _scalar / (MathF.PI * 2.0f), y, 0) + uiParent.position; 
            lineRenderer.SetPosition(i, position);
        }
    }

    float SoundFunction(float x)
    {
        return amplitude * (1.0f - Mathf.Pow(x / Mathf.PI, 2.0f)) *
               (0.5f + MathF.Pow(Mathf.Cos(x - Time.time * 6.0f - amplitude), 2.0f) / 2.0f) *
               Mathf.Sin((5.0f + 5.0f * amplitude) * x);
    }
}

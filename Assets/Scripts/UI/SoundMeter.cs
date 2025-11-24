using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class SoundMeter : MonoBehaviour
{
    public int resolution;
    public float soundLevel = 0.1f; // Sound from 0 to 1

    private float amplitude;
    private float _scalar;

    [SerializeField] private RectTransform uiParent;
    [SerializeField] private UILineRenderer lineRenderer;

    void Start()
    {
        _scalar = uiParent.localScale.x;
        amplitude = _scalar / 2.0f;
        lineRenderer = GetComponent<UILineRenderer>();
    }

    private void Update()
    {
        _scalar = uiParent.localScale.x;
        amplitude = soundLevel * MathF.PI;
        DrawWave();
    }

    void DrawWave()
    {
        Vector2[] points = new Vector2[resolution];

        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1);
            float x = t * Mathf.PI * 2.0f - Mathf.PI;
            float y = SoundFunction(x) * 0.5f * _scalar / MathF.PI;

            // Line world positions
            Vector3 worldPos = new Vector3( x * _scalar / (Mathf.PI * 2.0f), y, 0 ) + 
                               uiParent.position + uiParent.localScale/2.0f;

            // Convert world positions to ui positions
            Vector2 uiPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiParent, Camera.main.WorldToScreenPoint(worldPos),
                Camera.main, out uiPos
                );

            points[i] = uiPos;
        }

        lineRenderer.Points = points;
        lineRenderer.SetVerticesDirty();
    }

    float SoundFunction(float x)
    {
        return amplitude * (1.0f - Mathf.Pow(x / Mathf.PI, 2.0f)) *
               (0.5f + MathF.Pow(Mathf.Cos(x - Time.time * 5.0f - amplitude), 2.0f) / 2.0f) *
               Mathf.Sin((5.0f + 5.0f * amplitude) * x);
    }
}

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnemyParent : MonoBehaviour
{
    public Renderer[] renderers;
    public Light2D[] lights;

    public void SetVisible(bool v)
    {
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i]) renderers[i].enabled = v;

        for (int i = 0; i < lights.Length; i++)
            if (lights[i]) lights[i].enabled = v;
    }

    public Vector3 SamplePoint()
    {
        if (renderers != null && renderers.Length > 0 && renderers[0])
            return renderers[0].bounds.center;
        return transform.position;
    }
}

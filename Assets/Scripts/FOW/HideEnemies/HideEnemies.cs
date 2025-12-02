// HideEnemies.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HideEnemies : MonoBehaviour
{
    public LayerMask revealMask;
    public float probeRadius = 0.05f;
    public float refreshHz = 20f;

    float timer;
    readonly List<GameObject> enemies = new List<GameObject>();

    void Start()
    {
        var found = GameObject.FindGameObjectsWithTag("Enemy");
        enemies.Clear();
        enemies.AddRange(found);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer += 1f / Mathf.Max(1f, refreshHz);

        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (e == null) continue;

            Vector2 p = e.transform.position;
            bool visible = Physics2D.OverlapCircle(p, probeRadius, revealMask) != null;

            var rends = e.GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < rends.Length; r++) rends[r].enabled = visible;

            var lights = e.GetComponentsInChildren<Light2D>(true);
            for (int l = 0; l < lights.Length; l++) lights[l].enabled = visible;
        }
    }
}

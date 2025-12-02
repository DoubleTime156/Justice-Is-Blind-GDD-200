// CoinNoise.cs — fast version: trail + final pop, no occluded mesh, low CPU
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CoinNoise : MonoBehaviour
{
    public FogManager fogManager;

    public float trailRadiusWorld = 0.8f;
    public float trailHoldSeconds = 0.08f;
    public float trailHz = 18f;

    public float burstRadiusWorld = 6f;
    public float burstHoldSeconds = 0.8f;

    public bool revealOnFirstCollision = true;
    public bool useVelocityStop = true;
    public float stopSpeed = 0.15f;
    public float stopHoldTime = 0.10f;

    Rigidbody2D rb;
    bool fired;
    float stillTimer;
    float trailTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (fogManager == null) fogManager = FindObjectOfType<FogManager>();
    }

    void Update()
    {
        if (fogManager == null) return;

        if (!fired && trailRadiusWorld > 0f && trailHz > 0f)
        {
            trailTimer -= Time.deltaTime;
            if (trailTimer <= 0f)
            {
                fogManager.TriggerVisionBurstAt(transform.position,
                                                trailRadiusWorld,
                                                Mathf.Max(0.0001f, trailHoldSeconds),
                                                fogManager.defaultBurstFalloff);
                trailTimer += 1f / trailHz;
            }
        }

        if (fired || !useVelocityStop || rb == null) return;

        if (rb.linearVelocity.sqrMagnitude <= stopSpeed * stopSpeed)
        {
            stillTimer += Time.deltaTime;
            if (stillTimer >= stopHoldTime) Fire();
        }
        else
        {
            stillTimer = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D _)
    {
        if (!fired && revealOnFirstCollision) Fire();
    }

    void OnTriggerEnter2D(Collider2D _)
    {
        if (!fired && revealOnFirstCollision) Fire();
    }

    void Fire()
    {
        if (fogManager == null) return;
        fogManager.TriggerVisionBurstAt(transform.position,
                                        Mathf.Max(0f, burstRadiusWorld),
                                        Mathf.Max(0.0001f, burstHoldSeconds),
                                        fogManager.defaultBurstFalloff);
        fired = true;
    }
}

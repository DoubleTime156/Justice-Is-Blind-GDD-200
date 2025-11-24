using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CoinNoise : MonoBehaviour
{
    public FogManager fogManager;

    public float trailRadiusWorld = 0.8f;
    private float trailHold = 0.08f;
    private float trailHz = 20f;

    private float radiusWorld = 6f;
    private float whiteHold = 0.8f;

    private bool useVelocityStop = true;
    private float stopSpeed = 0.1f;
    private float stopHoldTime = 0.05f;
    private bool revealOnFirstCollision = true;

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
        if (!fired && fogManager != null && trailRadiusWorld > 0f && trailHz > 0f)
        {
            trailTimer -= Time.deltaTime;
            if (trailTimer <= 0f)
            {
                fogManager.TriggerVisionBurstAt(transform.position, trailRadiusWorld, Mathf.Max(0.0001f, trailHold), fogManager.defaultBurstFalloff);
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!fired && revealOnFirstCollision) Fire();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!fired && revealOnFirstCollision) Fire();
    }

    void Fire()
    {
        if (fogManager == null) return;
        fogManager.TriggerVisionBurstAt(transform.position, Mathf.Max(0f, radiusWorld), Mathf.Max(0.0001f, whiteHold), fogManager.defaultBurstFalloff);
        fired = true;
    }
}

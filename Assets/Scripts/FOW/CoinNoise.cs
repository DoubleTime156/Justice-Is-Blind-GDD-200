using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CoinNoise : MonoBehaviour
{
    public Material maskMaterial;
    public string fogMaskLayerName = "FogMask";
    public LayerMask obstacleMask;
    public int rayCount = 512;

    //size of the circle around the bottle when thrown, not when it breaks
    public float trailRadius = 0.8f;
    // how long that circle around the bottle is revealed
    public float trailTime = 0.08f;
    //How often it reveals around the bottle when thrown, keep high or its choppy
    public float trailVisibility = 100f;

    public float impactRadius = 6f;
    public float impactTime = 0.8f;

    private bool useVelocityStop = true;
    private float stopSpeed = 0.1f;
    private float stopTime = 0.05f;
    private bool revealOnFirstCollision = true;

    private ObjectSound objectSound;
    private bool logTrail = false;
    private bool logImpact = true;

    Rigidbody2D rb;
    bool fired;
    float stillTimer;
    float nextTrailTime;
    float impactTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!fired && maskMaterial != null && trailRadius > 0f && trailVisibility > 0f)
        {
            if (Time.time >= nextTrailTime)
            {
                if (!useVelocityStop || rb == null || rb.linearVelocity.sqrMagnitude > stopSpeed * stopSpeed)
                {
                    NoiseMask.Spawn(maskMaterial, fogMaskLayerName, obstacleMask, transform.position, trailRadius, trailTime, rayCount);
                    if (objectSound) objectSound.IsMakingSound = true;
                    if (logTrail) Debug.Log($"[CoinNoise] Trail at {transform.position} r={trailRadius} hold={trailTime}s");
                }
                nextTrailTime = Time.time + 1f / trailVisibility;
            }
        }

        if (!fired && useVelocityStop && rb != null)
        {
            if (rb.linearVelocity.sqrMagnitude <= stopSpeed * stopSpeed)
            {
                stillTimer += Time.deltaTime;
                if (stillTimer >= stopTime) Fire();
            }
            else
            {
                stillTimer = 0f;
            }
        }

        if (fired && impactTimer > 0f)
        {
            impactTimer -= Time.deltaTime;
            if (impactTimer <= 0f && objectSound) objectSound.IsMakingSound = false;
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
        if (maskMaterial == null) return;
        NoiseMask.Spawn(maskMaterial, fogMaskLayerName, obstacleMask, transform.position, impactRadius, impactTime, rayCount);
        if (objectSound) objectSound.IsMakingSound = true;
        impactTimer = impactTime;
        fired = true;
        if (logImpact) Debug.Log($"[CoinNoise] Impact at {transform.position} r={impactRadius} hold={impactTime}s");
    }
}

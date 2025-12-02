using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CoinNoise : MonoBehaviour
{
    public Material maskMaterial;
    public string fogMaskLayerName = "FogMask";
    public LayerMask obstacleMask;
    public int rayCount = 512;

    public float trailRadiusWorld = 0.8f;
    private float trailHold = 0.08f;
    private float trailHz = 20f;

    public float impactRadiusWorld = 6f;
    public float impactHold = 0.8f;

    private bool useVelocityStop = true;
    private float stopSpeed = 0.1f;
    private float stopHoldTime = 0.05f;
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
        if (!fired && maskMaterial != null && trailRadiusWorld > 0f && trailHz > 0f)
        {
            if (Time.time >= nextTrailTime)
            {
                if (!useVelocityStop || rb == null || rb.linearVelocity.sqrMagnitude > stopSpeed * stopSpeed)
                {
                    NoiseMask.Spawn(maskMaterial, fogMaskLayerName, obstacleMask, transform.position, trailRadiusWorld, trailHold, rayCount);
                    if (objectSound) objectSound.IsMakingSound = true;
                    if (logTrail) Debug.Log($"[CoinNoise] Trail at {transform.position} r={trailRadiusWorld} hold={trailHold}s");
                }
                nextTrailTime = Time.time + 1f / trailHz;
            }
        }

        if (!fired && useVelocityStop && rb != null)
        {
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
        NoiseMask.Spawn(maskMaterial, fogMaskLayerName, obstacleMask, transform.position, impactRadiusWorld, impactHold, rayCount);
        if (objectSound) objectSound.IsMakingSound = true;
        impactTimer = impactHold;
        fired = true;
        if (logImpact) Debug.Log($"[CoinNoise] Impact at {transform.position} r={impactRadiusWorld} hold={impactHold}s");
    }
}

using UnityEngine;

public class CoinNoise : MonoBehaviour
{
    public NoiseReveal noise;
    public float radiusWorld = 6f;
    public float whiteHold = 0.8f;
    public bool useVelocityStop = true;
    public float stopSpeed = 0.1f;
    public float stopHoldTime = 0.05f;

    private Rigidbody2D rb;
    private bool fired;
    private float stillTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
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
        if (!fired) Fire();
    }

    void Fire()
    {
        if (noise == null) noise = FindObjectOfType<NoiseReveal>();
        if (noise != null)
        {
            noise.holdSeconds = whiteHold;
            noise.RevealAtWorld(transform.position, radiusWorld);
        }
        fired = true;
    }
}

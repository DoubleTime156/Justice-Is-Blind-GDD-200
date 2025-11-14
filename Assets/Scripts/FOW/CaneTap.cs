using UnityEngine;

public class CaneTap : MonoBehaviour
{
    public NoiseReveal noiseReveal;
    public Transform player;

    public PlayerPosition playerVision;
    public FogManager fogManager;
    public float noiseMultiplier = 2f;
    public ObjectSound objectSound;
    public KeyCode tapKey = KeyCode.Space;

    public float tapRadiusWorld = 6f;
    public float tapHoldSeconds = 0.8f;

    public bool logTaps = true;

    private float tapCooldown;
    private bool canTap = true;

    void Update()
    {
        if (noiseReveal == null || player == null) return;

        if (Input.GetKeyDown(tapKey) && canTap)
        {
            float r = tapRadiusWorld > 0f ? tapRadiusWorld :
                      (playerVision != null ? playerVision.radius * Mathf.Max(0.01f, noiseMultiplier) : 6f);

        //    noiseReveal.holdSeconds = tapHoldSeconds;
            noiseReveal.RevealAtWorld(player.position, r);

            canTap = false;
            tapCooldown = tapHoldSeconds;

            if (logTaps) Debug.Log($"[CaneTap] Burst at {player.position}, r={r}, hold={tapHoldSeconds}s");
        }

        if (!canTap)
        {
            tapCooldown -= Time.deltaTime;
            if (tapCooldown <= 0f) canTap = true;
        }
    }

    void FixedUpdate()
    {
        if (objectSound != null) objectSound.IsMakingSound = !canTap;
    }
}

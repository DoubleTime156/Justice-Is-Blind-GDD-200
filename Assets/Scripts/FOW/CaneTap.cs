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

    public float tapFullBrightTime = 0.8f;

    public float tapFadeDuration = 2.0f;

    public bool logTaps = true;

    private float tapCooldown;
    private bool canTap = true;

    void Update()
    {
        if (noiseReveal == null || player == null) return;

        if (Input.GetKeyDown(tapKey) && canTap == true)
        {
            tapCooldown = tapFadeDuration;

            Vector2 tapWorld = new Vector2(player.position.x, player.position.y);
            canTap = false;

            noiseReveal.fullBrightTime = tapFullBrightTime;
            noiseReveal.fadeDuration = tapFadeDuration;

            noiseReveal.RevealAtWorld(tapWorld, tapRadiusWorld);

            if (logTaps)
            {
                Debug.Log($"[CaneTap] NoiseReveal at {tapWorld}, rWorld={tapRadiusWorld}, white {tapFullBrightTime}s then fade {tapFadeDuration}s");
            }
        }
        if (canTap == false)
        {
            tapCooldown -= Time.deltaTime;
            if (tapCooldown <= 1)
            {
                canTap = true; 
            }
        }

    }

    private void FixedUpdate()
    {
        objectSound.IsMakingSound = !canTap;
        Debug.Log(objectSound.IsMakingSound);
    }
}

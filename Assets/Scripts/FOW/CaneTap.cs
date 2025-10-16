using UnityEngine;
using System.Diagnostics;


public class CaneTap : MonoBehaviour
{

    public PlayerPosition playerVision;
    public FogManager fogManager;
    public float noiseMultiplier = 2f;
    public float noiseDuration = 1f;
    public float cooldown = 2f;
    private float originalRadius;
    private float timer = 0f;
    private float coolDownTimer = 0f;
    private bool isTapping = false;
    private bool canTap = true;
    void Update()
    {
        UnityEngine.Debug.Log("Cooldown" + coolDownTimer);

        UnityEngine.Debug.Log("Timer" + timer);

        if (Input.GetKeyDown(KeyCode.Space) && !isTapping && canTap)
        {
            originalRadius = playerVision.radius;
            playerVision.radius *= noiseMultiplier;

            if (fogManager != null)
            {
                fogManager.revealRadiusUV *= noiseMultiplier;
            }

            isTapping = true;
            canTap = false;
            timer = noiseDuration;
            coolDownTimer = cooldown;
        }
        if (!canTap)
        {
            coolDownTimer -= Time.deltaTime;
            if (coolDownTimer <= 0f)
            {
                canTap = true;
            }
        }
        if (isTapping)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                playerVision.radius = originalRadius;

                if (fogManager != null)
                {
                    fogManager.revealRadiusUV /= noiseMultiplier;

                 
                        isTapping = false;
                 
                }

            }
            
           

        }
    }
}

using UnityEngine;

public class CaneTap : MonoBehaviour
{

    public PlayerPosition playerVision;
    public FogManager fogManager;
    public float noiseMultiplier = 2f;
    public float noiseDuration = 0.1f;

    private float originalRadius;
    private float timer = 0f;
    private bool isTapping = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTapping)
        {
            originalRadius = playerVision.radius;
            playerVision.radius *= noiseMultiplier;

            if (fogManager != null)
            {
                fogManager.revealRadiusUV *= noiseMultiplier;
            }

            isTapping = true;
            timer = noiseDuration;
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

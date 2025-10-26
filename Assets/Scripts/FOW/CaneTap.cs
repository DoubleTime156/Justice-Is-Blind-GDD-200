using UnityEngine;

public class CaneTap : MonoBehaviour
{
    public GameObject soundPulsePrefab;  // Prefab with MeshRenderer + SoundPulse
    public Transform player;
    public float cooldown = 1f;

    private bool canTap = true;
    private float cooldownTimer = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canTap)
        {
            // Spawn a pulse at player's current position
            Instantiate(soundPulsePrefab, player.position, Quaternion.identity);

            canTap = false;
            cooldownTimer = cooldown;
        }

        if (!canTap)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
                canTap = true;
        }
    }
}

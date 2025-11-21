// CaneTap.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class CaneTap : MonoBehaviour
{
    public Transform player;
    public Material maskMaterial;
    public string fogMaskLayerName = "FogMask";
    public LayerMask obstacleMask;

    public KeyCode tapKey = KeyCode.Space;
    public float tapRadiusWorld = 6f;
    public float tapHoldSeconds = 0.8f;
    public int rayCount = 512;

    public ObjectSound objectSound;
    public bool logTaps = true;

    bool cooling;
    float timer;

    void Update()
    {
        if (player == null || maskMaterial == null) return;

        if (Input.GetKeyDown(tapKey) && !cooling)
        //if (Input.GetKeyDown(tapKey) && !cooling)
        //{
        //    NoiseMask.Spawn(maskMaterial, fogMaskLayerName, obstacleMask, player.position, tapRadiusWorld, tapHoldSeconds, rayCount);

        //    if (objectSound != null) objectSound.IsMakingSound = true;
        //    cooling = true;
        //    timer = tapHoldSeconds;

        //    if (logTaps) Debug.Log($"[CaneTap] NoiseMask at {player.position} r={tapRadiusWorld} hold={tapHoldSeconds}s");
        //}

        //if (cooling)
        //{
        //    timer -= Time.deltaTime;
        //    if (timer <= 0f)
        //    {
        //        cooling = false;
        //        if (objectSound != null) objectSound.IsMakingSound = false;
        //    }
        //}
    }

    public void onTap(InputAction.CallbackContext context)
    {
        if (context.performed && !cooling)
        {
            NoiseMask.Spawn(maskMaterial, fogMaskLayerName, obstacleMask, player.position, tapRadiusWorld, tapHoldSeconds, rayCount);

            if (objectSound != null) objectSound.IsMakingSound = true;
            cooling = true;
            timer = tapHoldSeconds;

            if (logTaps) Debug.Log($"[CaneTap] NoiseMask at {player.position} r={tapRadiusWorld} hold={tapHoldSeconds}s");
        }

        if (cooling)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                cooling = false;
                if (objectSound != null) objectSound.IsMakingSound = false;
            }
        }

    }

}

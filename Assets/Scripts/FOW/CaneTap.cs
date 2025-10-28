using UnityEngine;
using System.Diagnostics;


public class CaneTap : MonoBehaviour
{

    public PlayerPosition playerVision;
    public FogManager fogManager;
    public NoiseReveal noiseSystem;
    public Transform player;

    public float radius = 0.01f;

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            noiseSystem.RevealAt(player.position, radius);
            UnityEngine.Debug.Log("Cane Tapped");
        }

    }
}

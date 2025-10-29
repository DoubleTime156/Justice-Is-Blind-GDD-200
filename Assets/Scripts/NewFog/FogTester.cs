using UnityEngine;

public class FogTester : MonoBehaviour
{
    public NewFogManager fog;

    void Update()
    {
        // Constant vision around player
        fog.RevealCircle(transform.position, 0.05f);

        // Press Space for a bigger pulse
        if (Input.GetKeyDown(KeyCode.Space))
        {
            fog.RevealCircle(transform.position, 0.2f);
            Debug.Log("Revealing fog circle!");
        }
    }
}

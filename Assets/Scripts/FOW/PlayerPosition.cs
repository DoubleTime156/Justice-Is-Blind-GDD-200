using UnityEngine;

public class PlayerPosition : MonoBehaviour
{

    public Material visionMaterial;
    public Transform player;
    public float radius = 3f;
    public float falloff = 0.5f;

    void Update()
    {
        //gets players pos to set vision circle on player
        visionMaterial.SetVector("_PlayerPos", new Vector4(player.position.x, player.position.y, 0, 0));
        visionMaterial.SetFloat("_Radius", radius);
        visionMaterial.SetFloat("_Falloff", falloff);
    }
}

using UnityEngine;

public class FogPainter : MonoBehaviour
{
    public RenderTexture fogTexture;
    public Material paintMaterial; 
    public Transform player;
    public float radius = 3f;

    private void Update()
    {
        Vector3 pos = player.position;
        paintMaterial.SetVector("_Position", new Vector4(pos.x, pos.y, 0, 0));
        paintMaterial.SetFloat("_Radius", radius);


        RenderTexture temp = RenderTexture.GetTemporary(fogTexture.width, fogTexture.height, 0, fogTexture.format);
        Graphics.Blit(fogTexture, temp);
        Graphics.Blit(temp, fogTexture, paintMaterial);
        RenderTexture.ReleaseTemporary(temp);
    }
}

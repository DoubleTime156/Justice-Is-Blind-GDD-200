using UnityEngine;

public class SoundCircle : MonoBehaviour
{
    public Material soundMaterial;
    public float startRadius =  0f;
    public float endRadius = 5f;
    public float duration = 1.5f;

    private float timer = 0f;
    private Material matInstance;





    void Start()
    {
        matInstance = new Material(soundMaterial);
        GetComponent<MeshRenderer>().material = matInstance;

        matInstance.SetFloat("_Radius", startRadius);
        matInstance.SetFloat("_Falloff", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        //Expand the circle
        float currentRadius = Mathf.Lerp(startRadius, endRadius, t);
        matInstance.SetFloat("_Radius", currentRadius);
        //fade out
        float alpha = Mathf.Lerp(1f, 0f, t);
        matInstance.SetColor("_Darkness", new Color(0,0,0,alpha));

        if (t>=1f)
        {
            Destroy(gameObject); 
        }    
    }
}

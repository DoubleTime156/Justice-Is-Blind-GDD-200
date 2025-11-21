using UnityEngine;

public class JukeboxToggle : MonoBehaviour, BehaviorCallable
{
    [SerializeField]
    public AudioClip jukeMusic;

    [SerializeField]
    public AudioSource source;

    [SerializeField]
    private Sprite offJuke;

    [SerializeField]
    private Sprite onJuke;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        source.clip = jukeMusic;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Behavior()
    {
        if(source.isPlaying)
        {
            source.Stop();
            spriteRenderer.sprite = offJuke;
        } else
        {
            source.Play();
            spriteRenderer.sprite = onJuke;
        }
    }
}

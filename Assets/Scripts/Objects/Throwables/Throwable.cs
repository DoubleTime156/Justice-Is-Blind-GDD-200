using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    public FogManager fogManager;
    private float radiusWorld = 6f;
    private float whiteHold = 0.8f;

    private Vector3 target;
    private float speed;
    private int item;
    private Rigidbody2D rb;
    private float distance;
    private float targetRadius = 0.05f;
    private float[] forceMultiplier = { 1, 3 };
    private bool isTriggered = false;
    private float previousDistance;
    private float timeToReach;
    public ObjectSound objectSound;
    public AudioSource throwSound;
    public AudioSource impactSound;
    public bool inAir = true; // Bottles can kill enemies if active
    public void Init(Vector3 targetPos, float initMoveSpeed, int heldItem)
    {
        target = targetPos;
        speed = initMoveSpeed;
        item = heldItem;
        rb = GetComponent<Rigidbody2D>();
        distance = Vector2.Distance(transform.position, targetPos / forceMultiplier[heldItem]);
        timeToReach = distance / speed;
        throwSound.Play();
    }

    void FixedUpdate()
    {

        // Move towards target (Mouse position)
        Vector2 direction = (target - transform.position).normalized;
        if (item == 1) { rb.rotation += -15f; } //Apply spin to bottle
        distance = Vector2.Distance(transform.position, target);
        speed = distance / timeToReach;

        rb.linearVelocity = direction * speed;

        // When destination reached, perform item specific behaviors
        if (Vector2.Distance(transform.position, target) <= targetRadius && !isTriggered) //rb.linearVelocity.magnitude < 0.5
        {
            itemBehavior();
        }
        previousDistance = Vector2.Distance(transform.position, target);
    }

    private void itemBehavior()
    {
        StartCoroutine(stopMakingSound(0.1f));
        inAir = false;
        impactSound.Play();
        switch (item)
        {
            case 0:
                Debug.Log("Coin landed");
                objectSound.IsMakingSound = true;
             //   RevealFog(0.05f);
                rb.linearVelocity = new Vector2(0, 0); 
                isTriggered = true;
                break;
            case 1:
                Debug.Log("Bottle Landed");
                GetComponent<Renderer>().enabled = false;
                objectSound.IsMakingSound = true;
              //  RevealFog(0.07f);
                isTriggered = true;
                break;
        }
    }

    //void RevealFog(float radius)
   // {
   //     FogManager fog = FindFirstObjectByType<FogManager>();
   //     if (fog == null) return;
//
     //   FogRevealer revealer = gameObject.AddComponent<FogRevealer>();
   //     revealer.fogManager = fog;
   //     revealer.revealRadiusUV = radius;
    //    revealer.fullRevealDuration = 2f;
   //     revealer.fadeDuration = 3f;      
   //     revealer.TriggerReveal();
  //  }


    public void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("throwable hit wall!");
        inAir = false;
        switch (item)
        {
            case 0:
                itemBehavior();
                break;
            case 1:
                itemBehavior();
                break;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision) 
    {
        Debug.Log("Has Triggered Collider");
        if (collision.CompareTag("Enemy") && item == 1 && inAir) // If a bottle is still in air, destroy enemies they touch
        {
            Debug.Log("Has found an enemy collider tag");
            Destroy(collision.gameObject);
            Destroy(gameObject);
            fogManager.TriggerVisionBurstAt(transform.position, Mathf.Max(0f, radiusWorld), Mathf.Max(0.0001f, whiteHold), fogManager.defaultBurstFalloff);

        }
        if (collision.CompareTag("Enemy") && item == 0 && !inAir) // When an enemy inspects a coin, pick it up before going back to path
        {
            StartCoroutine(enemyPickupCoin(2.5f));
        }
    }

    IEnumerator stopMakingSound(float waitTime) // Makes sound to lure enemy and stops to prevent enemy jittering
    {
        objectSound.IsMakingSound = true;
        // Wait for the specified number of seconds
        yield return new WaitForSeconds(waitTime);

        // Stop making sound after time has past
        objectSound.IsMakingSound = false;
    }

    IEnumerator enemyPickupCoin(float waitTime) // When the enemy reaches the coin, the coin will wait a few seconds before disappearing
    { 
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }
}

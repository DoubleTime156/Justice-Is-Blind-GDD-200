using Unity.VisualScripting;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    private Vector3 target;
    private float speed;
    private int item;
    private Rigidbody2D rb;
    private float distance;
    private float targetRadius = 0.05f;
    private float[] forceMultiplier = { 1, 3 };

    public float timeToReach;
    public void Init(Vector3 targetPos, float initMoveSpeed, int heldItem)
    {
        target = targetPos;
        speed = initMoveSpeed;
        item = heldItem;
        rb = GetComponent<Rigidbody2D>();
        distance = Vector2.Distance(transform.position, targetPos / forceMultiplier[heldItem]);
        timeToReach = distance / speed;
    }

    void Update()
    {
        // Move towards target (Mouse position)
        Vector2 direction = (target - transform.position).normalized;
        distance = Vector2.Distance(transform.position, target);
        speed = distance / timeToReach;

        rb.linearVelocity = direction * speed;

        // When destination reached, perform behaviors
        if (Vector2.Distance(transform.position, target) <= targetRadius) //rb.linearVelocity.magnitude < 0.5
        {
            switch (item)
            {
                case 0:
                    rb.linearVelocity = new Vector2(0,0); break;
                case 1:
                    Destroy(gameObject); break;
            }
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("hit wall!");

        if (collision.gameObject.CompareTag("World"))
        {
            switch (item)
            {
                case 0:
                    break;
                case 1:
                    Destroy(gameObject); break;
            }
        }
    }
}

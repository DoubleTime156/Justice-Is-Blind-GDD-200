using Unity.VisualScripting;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    private Vector3 target;
    private float speed;
    private int item;
    private Rigidbody2D rb;
    private float distance;

    public float timeToReach = 1.5f;
    public void Init(Vector3 targetPos, float initMoveSpeed, int heldItem)
    {
        target = targetPos;
        speed = initMoveSpeed;
        item = heldItem;
        rb = GetComponent<Rigidbody2D>();
        distance = Vector2.Distance(transform.position, targetPos);
        timeToReach = distance / speed;
    }

    void Update()
    {
        // Move towards target (Mouse position)
        Vector2 direction = (target - transform.position).normalized;
        distance = Vector2.Distance(transform.position, target);
        speed = distance / timeToReach;

        // Option 1: Direct velocity
        rb.linearVelocity = direction * speed;

        // Option 2: Impulse force
        // rb.AddForce(direction * speed * rb.mass, ForceMode2D.Impulse);    

        // Stop when reached
        if (transform.position == target)
        {
            switch (item)
            {
                case 0:
                    enabled = false; break;
                case 1:
                    Destroy(gameObject); break;
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("hit wall!");

        if (collision.gameObject.CompareTag("World"))
        {
            CorrectCollision();
            switch (item)
            {
                case 0:
                    enabled = false;
                    break;
                case 1:
                    Destroy(gameObject); break;
            }
        }
    }

    private void CorrectCollision()
    {

    }

}

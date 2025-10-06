using UnityEngine;

public class Throwable : MonoBehaviour
{
    private Vector3 target;
    private float speed;
    private int item;
    private Rigidbody2D rb;

    public void Init(Vector3 targetPos, float moveSpeed, int heldItem)
    {
        target = targetPos;
        speed = moveSpeed;
        item = heldItem;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Move towards target (Mouse position)
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

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

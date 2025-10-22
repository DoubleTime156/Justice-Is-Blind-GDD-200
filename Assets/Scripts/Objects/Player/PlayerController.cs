using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D_InputSystem : MonoBehaviour
{
    public float moveSpeed;

    private Rigidbody2D rb;
    private Vector2 movement;
    public int[] inventory = { 0, 0 }; //{Coins held, Bottles Held}
    public bool hasKey = false;
    private GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            gameManager.gameOver();
            //Destroy(gameObject);

        // Handle key pickup
        } else if (collision.gameObject.CompareTag("Pickup") && collision.gameObject.name == "Key")
        {
            Destroy(collision.gameObject);
            hasKey = true;
        }
    }
}

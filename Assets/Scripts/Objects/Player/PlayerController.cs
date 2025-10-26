using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D_InputSystem : MonoBehaviour
{
    public PlayerData data;

    private Rigidbody2D rb;
    private Vector2 movement;
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
        rb.MovePosition(rb.position + movement * data.moveSpeed);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            gameManager.gameOver();
            //Destroy(gameObject);
        }else if (collision.gameObject.CompareTag("Pickup"))
        {
            data.inventory[collision.GetComponent<Pickup>().pickupType]++;
            gameManager.updateAmount();
            Debug.Log("Pickup collected");
            Destroy(collision.gameObject);
        }
    }
}

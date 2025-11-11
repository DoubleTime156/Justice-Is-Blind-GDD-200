using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D_InputSystem : MonoBehaviour
{
    public PlayerData data;

    private Rigidbody2D rb;
    private Vector2 movement;
    private GameManager gameManager;
    public AudioSource coinPickup;
    public AudioSource bottlePickup;

    private int[] defaultInventory = { 0, 0 };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();

        // Reset PlayerData
        data.heldItem = 0;
        data.inventory = defaultInventory;
        data.hasKey = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * data.moveSpeed * data.moveMulti);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            gameManager.gameOver();
            this.enabled = false;
        }else if (collision.gameObject.CompareTag("Pickup"))
        {
            data.inventory[collision.GetComponent<Pickup>().pickupType]++;
            gameManager.updateAmount();
            Debug.Log("Pickup collected");
            switch (collision.GetComponent<Pickup>().pickupType) // Play audio for pickups
            {
                case 0:
                    coinPickup.Play();
                    break;
                case 1:
                    bottlePickup.Play();
                    break;
            }   
            Destroy(collision.gameObject);

        } else if (collision.gameObject.CompareTag("Key") && !data.hasKey)
        {
            data.hasKey = true;
            Destroy(collision.gameObject);
        }
    }

    public void onSneak(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            data.moveMulti = 0.5f;
        }
        else if (context.canceled)
        {
            data.moveMulti = 1.0f;
        }

    }

    public void onSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            data.moveMulti = 2.0f;
        }
        else if (context.canceled)
        {
            data.moveMulti = 1.0f;
        }
    }
    
}

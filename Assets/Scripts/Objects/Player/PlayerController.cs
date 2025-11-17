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
    private Inventory inventoryUI;
    public AudioSource coinPickup;
    public AudioSource bottlePickup;
    public AudioSource keyPickup;

    private int[] defaultInventory = { 0, 0 };

    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        inventoryUI = GameObject.Find("Inventory").GetComponent<Inventory>();

        // Reset PlayerData
        data.heldItem = 1;
        data.inventory = defaultInventory;
        data.hasKey = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();

        //for movement animation
        if(movement.y != 0)
        {
            //is moving horizontally
            animator.SetBool("isMovingVert", true);

        }
        else if(movement.x != 0)
        {
            //is moving vertically
            animator.SetBool("isMovingHoriz", true);
        }
        else
        {
            //not moving, make bool false
            animator.SetBool("isMovingHoriz", false);
            animator.SetBool("isMovingVert", false);
        }
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
            this.enabled = false;
        }
        else if (collision.gameObject.CompareTag("Pickup"))
        {
            data.inventory[collision.GetComponent<Pickup>().pickupType]++;
            inventoryUI.updateAmount();
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

        }
        else if (collision.gameObject.CompareTag("Key") && !data.hasKey)
        {
            keyPickup.Play();
            data.hasKey = true;
            inventoryUI.updateAmount();
            Destroy(collision.gameObject);
        }
    }
}

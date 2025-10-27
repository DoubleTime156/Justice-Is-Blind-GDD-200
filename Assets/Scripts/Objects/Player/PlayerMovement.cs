using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D_InputSystem : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float speedMulti;

    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    public int[] inventory = { 0, 0 }; //{Coins held, Bottles Held}
    private GameManager gameManager;

    //player input
    public PlayerInputActions playerControls;
    //player movement
    private InputAction move;
    private InputAction sprint;
    private InputAction sneak;

    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }

    private void OnEnable()
    {
        //movement set up
        move = playerControls.Player.Move;
        move.Enable();

        //sprint set up
        sprint = playerControls.Player.Run;
        sprint.Enable();
        sprint.performed += SprintHold;
        sprint.canceled += SprintRelease;

        //sneak set up
        sneak = playerControls.Player.Sneak;
        sneak.Enable();
        sneak.performed += sneakHold;
        sneak.canceled += sneakRelease;
    }

    private void OnDisable()
    {
        //disabling controls once play ends
        move.Disable();
        sprint.Disable();
        sneak.Disable();
    }

        void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
    }

    void Update()
    {
        //getting input for moveDireciton
        moveDirection = move.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed * speedMulti, moveDirection.y * moveSpeed * speedMulti);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            gameManager.gameOver();
            //Destroy(gameObject);
        }
    }

    //sprinting logic
    private void SprintHold(InputAction.CallbackContext context)
    {
        speedMulti = 2.0f; //increases movement speed
    }
    private void SprintRelease(InputAction.CallbackContext context)
    {
        speedMulti = 1.0f; //resets movement speed to normal
    }

    //sneak logic
    private void sneakHold(InputAction.CallbackContext context)
    {
        speedMulti = 0.5f; //decreases movement speed
    }
    private void sneakRelease(InputAction.CallbackContext context)
    {
        speedMulti = 1.0f; //returns movement to normal
    }
}

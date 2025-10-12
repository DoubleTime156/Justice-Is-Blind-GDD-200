using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    //rigid body
    public Rigidbody2D rb;

    //movement variables
    public float speed = 5.0f;
    public float speedMulti = 1.0f;
    Vector2 moveDirection = Vector2.zero;
   
    //player input
    public PlayerInputActions playerControls;

    //specific controls
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
        sprint = playerControls.Player.Sprint;
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
    }

    void Start()
    {
        
    }

    void Update()
    {
       //getting input for moveDireciton
       moveDirection = move.ReadValue<Vector2>();   
    }

    private void FixedUpdate()
    {
        //moving the player
        rb.linearVelocity = new Vector2(moveDirection.x * speed * speedMulti, moveDirection.y * speed * speedMulti);
    }
    
    //sprinting logic
    private void SprintHold(InputAction.CallbackContext context)
    {
        speedMulti = 2.0f;
    }
    private void SprintRelease(InputAction.CallbackContext context)
    {
        speedMulti = 1.0f;
    }

    //sneak logic
    private void sneakHold(InputAction.CallbackContext context)
    {
        speedMulti = 0.5f;
    }
    private void sneakRelease(InputAction.CallbackContext context)
    {
        speedMulti = 1.0f;
    }


}

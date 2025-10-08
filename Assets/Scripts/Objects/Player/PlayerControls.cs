using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{

    public Rigidbody2D rb;
    public float speed = 5.0f;
    public float speedMult = 1.0f;
    public InputAction playerMovement;
    public InputAction playerRun;

    Vector2 moveDirection = Vector2.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        playerMovement.Enable();
        playerRun.Enable();
    }

    private void OnDisable()
    {
        playerMovement.Disable();
        playerRun.Disable();
    }


    // Update is called once per frame
    void Update()
    {
        moveDirection = playerMovement.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * speed * speedMult, moveDirection.y * speed * speedMult);
    }

    private void Run(InputAction.CallbackContext context)
    {
        speedMult = 2.0f;
    }
}

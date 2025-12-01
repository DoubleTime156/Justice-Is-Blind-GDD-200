using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerSwing : MonoBehaviour {

    public float playerViewRadius;
    public float playerViewAngle;
    public ParticleSystem knockoutParticles;

    public void OnSwing(InputAction.CallbackContext context)
    {
        // Only handle swing when the action is performed
        if (!context.performed) return;
        
        // Find all enemies inside the view radius
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, playerViewRadius, LayerMask.GetMask("Enemy"));

        // Get mouse world position and rotate player to face it
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        //transform.rotation = Quaternion.LookRotation(Vector3.forward, mouseWorldPos - transform.position);

        // Collect only the valid targets (in angle and not blocked)
        var targets = new System.Collections.Generic.List<Collider2D>();

        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            if (enemyCollider == null) continue;

            Vector2 dir = (Vector2)(enemyCollider.transform.position - transform.position);
            float distance = dir.magnitude;
            if (distance <= 0f) continue;
            dir.Normalize();

            float angle = Vector2.Angle(transform.up, dir);

            if (angle <= playerViewAngle)
            {
                // Raycast towards the enemy to check for obstacles
                RaycastHit2D hitObstacle = Physics2D.Raycast(transform.position, dir, distance, LayerMask.GetMask("Obstacle"));

                // If no obstacle hit, mark this enemy as a valid target, and get enemy data
                if (hitObstacle.collider == null)
                {
                    targets.Add(enemyCollider);
                    //EnemyData enemyData = enemyCollider.GetComponent<EnemyData>();
                }
            }
        }

        // Destroy only the valid targets collected above
        if (targets.Count > 0)
        {
            foreach (var target in targets)
            {
                if (target != null)
                knockoutParticles = Instantiate(knockoutParticles, target.transform.position, Quaternion.identity);
                Destroy(target.gameObject);    
            }
        }
    }

    // Debug - Vision cone visual
    void OnDrawGizmos()
    {
        // Optional: visualize the cone in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerViewRadius);

        Vector3 leftDir = Quaternion.Euler(0, 0, playerViewAngle) * transform.up * playerViewRadius;
        Vector3 rightDir = Quaternion.Euler(0, 0, -playerViewAngle) * transform.up * playerViewRadius;
        Gizmos.DrawLine(transform.position, transform.position + leftDir);
        Gizmos.DrawLine(transform.position, transform.position + rightDir);
    }
}


/*
public class PlayerSwing : MonoBehaviour
{
    public GameObject Enemy;
    public GameObject SwingRange;
    //public PlayerActions playerSwing;

    bool enemyIsInRange;

    private InputAction swing;
    private PlayerController2D_InputSystem playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController2D_InputSystem>();
        enemyIsInRange = false;
    }

   


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Enemy)
        {
            Debug.Log("Enemy hit by swing!");
            // Add logic for damaging the enemy here
            enemyIsInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == Enemy)
        {
            Debug.Log("Enemy exited swing area.");
            // Add logic for when the enemy exits the swing area here
        }
    }

    public void OnSwing(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Player swung weapon!");
            // Add swing logic here
            if (enemyIsInRange) {
                Destroy(Enemy);
                Debug.Log("Enemy destroyed by swing!");

            }
        }   
    }
}
*/

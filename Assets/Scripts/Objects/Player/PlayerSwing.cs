using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerSwing : MonoBehaviour {

    public float playerViewRadius;
    public float playerViewAngle;


    public void OnSwing(InputAction.CallbackContext context)
    {
        // Only handle swing when the action is performed
        if (!context.performed) return;
        
        // Find all enemies inside the view radius
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, playerViewRadius, LayerMask.GetMask("Enemy"));

        // Get mouse world position and rotate player to face its
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, mouseWorldPos - transform.position);

        // Collect only the valid targets (in angle and not blocked)
        var enemies = new System.Collections.Generic.List<Collider2D>();

        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            if (enemyCollider == null) continue;

            Vector2 dir = (Vector2)(enemyCollider.transform.position - transform.position);
            float distance = dir.magnitude;
            if (distance <= 0f) continue;
            dir.Normalize();

            // Get current enemy health
            EnemyData enemyHealthData = ScriptableObject.CreateInstance<EnemyData>();
            enemyHealthData = enemyCollider.GetComponent<EnemyData>();

            float angle = Vector2.Angle(transform.up, dir);


            if (angle <= playerViewAngle)
            {
                // Raycast towards the enemy to check for obstacles
                RaycastHit2D hitObstacle = Physics2D.Raycast(transform.position, dir, distance, LayerMask.GetMask("Obstacle"));

                // If no obstacle hit, mark this enemy as a valid target, and get enemy data
                if (hitObstacle.collider == null)
                {
                    enemies.Add(enemyCollider);
                    // Get current enemy health
                    Debug.Log(enemyCollider.name + " Current Health: " + enemyHealthData);
                }
            }
        }

        // Destroy only the valid targets collected above and if enemy healtlh equals zero
        if (enemies.Count > 0)
        {
            foreach (var target in enemies)
            {
                if (target != null)
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



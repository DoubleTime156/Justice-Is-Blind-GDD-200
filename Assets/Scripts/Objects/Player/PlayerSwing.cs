using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;


public class PlayerSwing : MonoBehaviour {

    public float playerViewRadius;
    public float playerViewAngle;

    private GameObject swingRange;

    private void Awake()
    {
        swingRange = transform.Find("SwingRange").gameObject;
    }

    public void OnSwing(InputAction.CallbackContext context)
    {
        // Only handle swing when the action is performed
        if (!context.performed) return;
        
        // Find all enemies inside the view radius
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(swingRange.transform.position, playerViewRadius, LayerMask.GetMask("Enemy"));

        // Get mouse world position and rotate player to face it
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(swingRange.transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector3 direction = mouseWorldPos - swingRange.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        swingRange.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Collect only the valid targets (in angle and not blocked)
        var enemies = new System.Collections.Generic.List<Collider2D>();

        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            if (enemyCollider == null) continue;

            Vector2 dir = (Vector2)(enemyCollider.transform.position - swingRange.transform.position);
            float distance = dir.magnitude;
            if (distance <= 0f) continue;
            dir.Normalize();

            float angleToEnemy = Vector2.Angle(swingRange.transform.up, dir);

            if (angleToEnemy <= playerViewAngle)
            {
                // Raycast towards the enemy to check for obstacles
                RaycastHit2D hitObstacle = Physics2D.Raycast(swingRange.transform.position, dir, distance, LayerMask.GetMask("Obstacle"));

                // If no obstacle hit, mark this enemy as a valid target, and get enemy data
                if (hitObstacle.collider == null)
                {
                    enemies.Add(enemyCollider);
                }
            }
        }

        // Destroy only the valid targets collected above and if enemy healtlh equals zero
        if (enemies.Count > 0)
        {
            foreach (var target in enemies)
            {
                if (target != null)
                {
                    // Rotates the whole enemy, disable its AI,and collider
                    target.transform.rotation = Quaternion.Euler(0, 0, 0);
                    target.transform.GetChild(0).rotation = Quaternion.Euler(0, 0, -90); // Gets enemy sprite child and rotates it
                    target.GetComponent<EnemyAI>().enabled = false;
                    target.GetComponent<Collider2D>().enabled = false;
                    target.GetComponentInChildren<Light2D>().enabled = false;

                    // Tint the sprite
                    SpriteRenderer sr = null;
                    foreach (var renderer in target.GetComponentsInChildren<SpriteRenderer>())
                    {
                        if (renderer.gameObject.name == "Goon")
                        {
                            sr = renderer;
                            break;
                        }
                    }
                    if (sr != null)
                    {
                        sr.color = Color.red;
                    }
                }
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



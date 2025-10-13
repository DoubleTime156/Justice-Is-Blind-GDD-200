using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public Transform player;
    public EnemyPathfindingDebugger mover;
    public float viewRadius = 5f;
    public float viewAngle = 60f;

    void FixedUpdate()
    {
        Vector2 dir = player.position - transform.position;
        float distance = dir.magnitude;
        dir = dir.normalized;

        if (distance <= viewRadius)
        {
            float angle = Vector2.Angle(transform.up, dir);

            if (angle <= viewAngle)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, 
                    LayerMask.GetMask("Obstacle")); // Check for obstacles

                if (!hit)
                {
                    mover.UpdateDirection(dir);
                    mover.UpdateMovement();
                }
            }
        }
    }

    // Debug - Vision cone visual
    void OnDrawGizmos()
    {
        // Optional: visualize the cone in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftDir = Quaternion.Euler(0, 0, viewAngle) * transform.up * viewRadius;
        Vector3 rightDir = Quaternion.Euler(0, 0, -viewAngle) * transform.up * viewRadius;
        Gizmos.DrawLine(transform.position, transform.position + leftDir);
        Gizmos.DrawLine(transform.position, transform.position + rightDir);
    }
}
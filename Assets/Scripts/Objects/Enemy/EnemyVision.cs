using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public float viewRadius;
    public float viewAngle;
    private GameObject _lightVision;

    public bool CanSeeTarget { get; private set; }
    public Vector2 TargetDir { get; private set; }

    void Awake()
    {
        _lightVision = transform.Find("LightVision").gameObject;
    }

    public void UpdateVision(Vector3 targetPos)
    {
        TargetDir = targetPos - transform.position;
        float distance = TargetDir.magnitude;
        TargetDir = TargetDir.normalized;
        CanSeeTarget = false;

        if (distance <= viewRadius)
        {
            float angle = Vector2.Angle(_lightVision.transform.up, TargetDir);

            if (angle <= viewAngle)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, TargetDir, distance, 
                    LayerMask.GetMask("Obstacle")); // Check for obstacles

                if (!hit)
                {
                    CanSeeTarget = true;
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
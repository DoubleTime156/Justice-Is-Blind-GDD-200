using UnityEngine;
using System.Collections.Generic;

public class EnemyPathfindingDebugger : MonoBehaviour
{
    public Transform player;
    public Pathfinding pathfinder;
    public float speed = 3f;
    public float nextNodeDistance = 0.5f;
    public float pathUpdateInterval = 0.2f; // seconds
    public float rotateSpeed = 3f;

    private List<Node> path;
    private int currentNodeIndex;
    private float lastPathUpdateTime;

    void Start()
    {
        lastPathUpdateTime = -pathUpdateInterval;
    }

    public void UpdateMovement()
    {
        if (Time.time - lastPathUpdateTime >= pathUpdateInterval)
        {
            UpdatePath();
            lastPathUpdateTime = Time.time;
        }

        // Move along path if it exists and within distance of player
        if (path != null && path.Count > 0 && (player.position - transform.position).magnitude <= 13.0f)
        {
            Vector3 targetPos = path[currentNodeIndex].worldPosition;
            Vector3 dir = (targetPos - transform.position).normalized;
            transform.position += speed * Time.deltaTime * dir;

            if (Vector3.Distance(transform.position, targetPos) < nextNodeDistance)
            {
                currentNodeIndex++;
                if (currentNodeIndex >= path.Count)
                {
                    currentNodeIndex = path.Count - 1;
                }
            }
        }
    }

    public void UpdateDirection(Vector2 dir)
    {
        Vector2 newDir = Vector2.Lerp(transform.up, dir.normalized, rotateSpeed);
        transform.up = newDir;
    }

    void UpdatePath()
    {
        if (player != null && pathfinder != null)
        {
            path = pathfinder.FindPath(transform.position, player.position);
            currentNodeIndex = 0;
        }
    }

    // Debug - Path visual
    void OnDrawGizmos()
    {
        if (path == null) return;

        Gizmos.color = Color.red;

        for (int i = 0; i < path.Count; i++)
        {
            Gizmos.DrawSphere(path[i].worldPosition + Vector3.up * 0.1f, 0.1f);

            if (i < path.Count - 1)
                Gizmos.DrawLine(path[i].worldPosition + Vector3.up * 0.1f,
                    path[i + 1].worldPosition + Vector3.up * 0.1f);
        }
    }
    
}
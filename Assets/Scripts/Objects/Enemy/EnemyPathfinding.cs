using UnityEngine;
using System.Collections.Generic;

public class EnemyPathfinding : MonoBehaviour
{
    public EnemyData data;
    public Transform player;
    public Pathfinding pathfinder;
    public float nextNodeDistance = 0.5f;
    public float pathUpdateInterval = 0.2f; // seconds

    private float _speed;
    private float _rotateSpeed;
    private List<Node> _path;
    private int _atNode;
    private float _lastPathUpdateTime;

    void Awake()
    {
        _speed = data.chaseSpeed;
        _rotateSpeed = data.rotateSpeed;

        _lastPathUpdateTime = -pathUpdateInterval;
    }

    public void UpdateMovement()
    {
        if (Time.time - _lastPathUpdateTime >= pathUpdateInterval)
        {
            UpdatePath();
            _lastPathUpdateTime = Time.time;
        }

        // Move along _path if it exists and within distance of player
        if (_path != null && _path.Count > 0 && (player.position - transform.position).magnitude <= 13.0f)
        {
            Vector3 targetPos = _path[_atNode].worldPosition;
            Vector3 dir = (targetPos - transform.position).normalized;
            transform.position += _speed * dir;

            if (Vector3.Distance(transform.position, targetPos) < nextNodeDistance)
            {
                _atNode++;
                if (_atNode >= _path.Count)
                {
                    _atNode = _path.Count - 1;
                }
            }
        }
    }

    public void UpdateDirection(Vector2 dir)
    {
        Vector2 newDir = Vector2.Lerp(transform.up, dir.normalized, _rotateSpeed);
        transform.up = newDir;
    }

    void UpdatePath()
    {
        if (player != null && pathfinder != null)
        {
            _path = pathfinder.FindPath(transform.position, player.position);
            _atNode = 0;
        }
    }

    // Debug - Path visual
    void OnDrawGizmos()
    {
        if (_path == null) return;

        Gizmos.color = Color.red;

        for (int i = 0; i < _path.Count; i++)
        {
            Gizmos.DrawSphere(_path[i].worldPosition + Vector3.up * 0.1f, 0.1f);

            if (i < _path.Count - 1)
                Gizmos.DrawLine(_path[i].worldPosition + Vector3.up * 0.1f,
                    _path[i + 1].worldPosition + Vector3.up * 0.1f);
        }
    }
    
}
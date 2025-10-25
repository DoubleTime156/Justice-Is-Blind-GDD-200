using UnityEngine;
using System.Collections.Generic;

public class EnemyPathfinding : MonoBehaviour
{
    public EnemyData data;
    public Pathfinding pathfinder;
    public float nextNodeDistance;

    private List<Node> _path;
    private int _atNode;

    public Vector3 TargetPos { get; private set; }
    public Vector2 TargetDir { get; private set; }


    public void SetTargetNodeOnPath()
    {
        if (_path == null || _path.Count < 0) return;

        // Set target position and direction to node
        TargetPos = _path.Count > 0 
            ? _path[_atNode].worldPosition 
            : pathfinder.TargetPos;
        TargetDir = (TargetPos - transform.position).normalized;
    }

    public void SetNextTargetNode()
    {
        if (_path == null || _path.Count <= 0) return;

        // Check if arrived at next node, if so, set the next node
        if (Vector3.Distance(transform.position, TargetPos) < nextNodeDistance)
        {
            _atNode++;
            if (_atNode >= _path.Count)
            {
                _atNode = _path.Count - 1;
            }
        }
    }

    public void UpdatePath(Vector3 targetPos)
    {
        if (pathfinder != null)
        {
            _path = pathfinder.FindPath(transform.position, targetPos);
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
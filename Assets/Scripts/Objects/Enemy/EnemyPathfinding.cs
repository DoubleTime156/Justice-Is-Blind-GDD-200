using UnityEngine;
using System.Collections.Generic;

public class EnemyPathfinding : MonoBehaviour
{
    public EnemyData data;
    public Pathfinding pathfinder;
    public float nextNodeDistance;

    private List<Node> _path;
    private int _nextNode;

    public Vector3 TargetPos { get; private set; }
    public Vector2 TargetDir { get; private set; }

    public void UpdatePath(Vector3 targetPos)
    {
        if (pathfinder == null) return;

        // Don't create path if already next to target
        if (Vector3.Distance(transform.position, targetPos) <= 1f)
        {
            _path = null;
            TargetPos = targetPos;
            TargetDir = (TargetPos - transform.position).normalized;
            return;
        }

        // Calculate path
        List<Node> newPath = pathfinder.FindPath(transform.position, targetPos);
        if (newPath == null || newPath.Count == 0) return;

        // Keep the current node if it's still close in the new path
        if (_path != null && _nextNode < _path.Count)
        {
            Node curNode = _path[_nextNode];
            int closestIndex = newPath.FindIndex(n => Vector3.Distance(n.worldPosition, curNode.worldPosition) < data.chaseSpeed);
            _nextNode = Mathf.Clamp(closestIndex, 0, newPath.Count - 1);
        }
        else
        {
            _nextNode = 0;
        }

        _path = newPath;
    }

    public void SetTargetNodeTransforms()
    {
        if (_path == null || _path.Count <= 0) return;

        // Set target transforms
        TargetPos = _path[_nextNode].worldPosition;
        TargetDir = (TargetPos - transform.position).normalized;
    }

    public void SetNextTargetNode()
    {
        if (_path == null || _path.Count <= 0) return;

        // Check if arrived at next node, if so, set the next node
        if (Vector3.Distance(transform.position, TargetPos) < nextNodeDistance)
        {
            _nextNode++;
            if (_nextNode >= _path.Count)
            {
                _nextNode = _path.Count - 1;
            }
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
using System;
using System.IO;
using System.Linq;
using UnityEngine;


public class EnemyRoaming : MonoBehaviour
{
    public EnemyData data;

    private int _atNode = 0;
    private float _roamingSpeed;
    private float _rotateSpeed;

    public string roamType; // [empty for no path], "path", "circular"
    public Transform[] nodes;


    private void Awake()
    {
        _roamingSpeed = data.roamingSpeed;
        _rotateSpeed = data.rotateSpeed;

        if (nodes.Length == 0)
        {
            GameObject node = new GameObject("Node");
            node.transform.position = transform.position;
            node.transform.rotation = transform.rotation;
            
            // Later store initial nodes in a parent manager
            //node.transform.parent = null;

            nodes = new Transform[1];
            nodes[0] = node.transform;
        }
    }

    public void UpdateMovement()
    {
        switch (roamType)
        {
            case "path":
                if (nodes.Length > 1) 
                    FollowNodes();
                else 
                    Debug.LogError("Not enough nodes to form path");

                break;
            case "circular":
                if (nodes.Length == 1)
                {
                    // Move around single node
                }
                else if (nodes.Length < 1)
                {
                    Debug.LogError("Missing nodes!");
                }
                else
                {
                    Debug.LogError("\"Circular\" only needs one node!");
                }
                break;
            default:
                if (Vector3.Distance(transform.position, nodes[0].position) >= _roamingSpeed)
                    FollowNodes();
                else
                    transform.rotation = Quaternion.Lerp(transform.rotation, nodes[0].rotation, _rotateSpeed);
                break;
        }
    }


    private void FollowNodes()
    {
        Transform target = nodes[_atNode];
        Vector3 dir = (target.position - transform.position).normalized;

        // When pathfinding script is ready, use A* for each node instead if raycast to next code is hit
        transform.position += _roamingSpeed * dir;
        Vector2 newDir = Vector2.Lerp(transform.up, dir.normalized, _rotateSpeed);
        transform.up = newDir;

        if (Vector3.Distance(transform.position, target.position) < _roamingSpeed
            && roamType == "path")
        {
            _atNode++;
            if (_atNode >= nodes.Length)
            {
                _atNode = 0;
            }
        }
    }
}

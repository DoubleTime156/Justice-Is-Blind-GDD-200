using System;
using System.IO;
using System.Linq;
using UnityEngine;


public class EnemyRoaming : MonoBehaviour
{
    public EnemyData data;

    private float _roamingSpeed;
    private float _rotateSpeed;
    private GameObject _lightVision;

    public int AtNode { get; private set; }

    public string RoamType; // [empty for no path], "path", "circular"
    public Transform[] Nodes;


    private void Awake()
    {
        _roamingSpeed = data.roamingSpeed;
        _rotateSpeed = data.rotateSpeed;
        _lightVision = transform.Find("LightVision").gameObject;

        AtNode = 0;

        if (Nodes.Length == 0)
        {
            GameObject node = new GameObject("Node");
            node.transform.position = transform.position;
            node.transform.rotation = _lightVision.transform.rotation;
            
            // Later store initial Nodes in a parent manager
            //node.transform.parent = null;

            Nodes = new Transform[1];
            Nodes[0] = node.transform;
        }
    }

    public void UpdateMovement()
    {
        switch (RoamType)
        {
            case "path":
                if (Nodes.Length > 1) 
                    FollowNodes();
                else 
                    Debug.LogError("Not enough Nodes to form path");

                break;
            case "circular":
                if (Nodes.Length == 1)
                {
                    // Move around single node
                }
                else if (Nodes.Length < 1)
                {
                    Debug.LogError("Missing Nodes!");
                }
                else
                {
                    Debug.LogError("\"Circular\" only needs one node!");
                }
                break;
            default:
                if (Vector3.Distance(transform.position, Nodes[0].position) >= _roamingSpeed)
                    FollowNodes();
                else
                    _lightVision.transform.rotation = Quaternion.Lerp(_lightVision.transform.rotation, Nodes[0].rotation, _rotateSpeed);
                break;
        }
    }


    private void FollowNodes()
    {
        Transform target = Nodes[AtNode];
        Vector3 dir = (target.position - transform.position).normalized;

        // When pathfinding script is ready, use A* for each node instead if raycast to next code is hit
        transform.position += _roamingSpeed * dir;
        Vector2 newDir = Vector2.Lerp(_lightVision.transform.up, dir.normalized, _rotateSpeed);
        _lightVision.transform.up = newDir;

        if (Vector3.Distance(transform.position, target.position) < _roamingSpeed
            && RoamType == "path")
        {
            AtNode++;
            if (AtNode >= Nodes.Length)
            {
                AtNode = 0;
            }
        }
    }
}

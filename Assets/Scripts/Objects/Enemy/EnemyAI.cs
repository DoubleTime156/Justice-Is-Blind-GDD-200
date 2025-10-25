using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public bool IsChasing { get; private set; }

    [SerializeField] private EnemyData data;
    [SerializeField] private EnemyTransformer transformer;
    [SerializeField] private EnemyVision vision;
    [SerializeField] private EnemyPathfinding pathfind;
    [SerializeField] private EnemyRoaming roam;

    private GameObject player;
    private Vector3 lastKnownPos;

    private Vector3 dir;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) Debug.LogError("Missing Player Object!");

        IsChasing = false;
    }

    void FixedUpdate()
    {
        vision.UpdateVision(player.transform);

        if (vision.CanSeeTarget /* || "hears sound" */)
        {
            IsChasing = true;
            lastKnownPos = player.transform.position;
        }
        else if (Vector3.Distance(transform.position, lastKnownPos) <= data.chaseSpeed)
        {
            IsChasing = false;
        }

        if (IsChasing)
        {
            transformer.SetSpeed(data.chaseSpeed);
            if (vision.CanSeeTarget)
            {
                dir = vision.TargetDir;
            }
            else
            {
                pathfind.UpdatePath(lastKnownPos);
                pathfind.SetTargetNodeOnPath();
                dir = pathfind.TargetDir;
            }
        }
        else
        {
            transformer.SetSpeed(data.roamingSpeed);

            pathfind.UpdatePath(roam.nodes[0].position);
            pathfind.SetTargetNodeOnPath();
            dir = pathfind.TargetDir;

            //roam.UpdateMovement();
        }

        // Transform Enemy
        transformer.UpdateMovement(dir);
        transformer.UpdateDirection(dir);

        // Check any changes after transform
        pathfind.SetNextTargetNode();
    }
}

using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public bool IsChasing { get; private set; }
    public bool IsRoaming { get; private set; }

    [SerializeField] private EnemyData data;
    [SerializeField] private EnemyVision vision;
    [SerializeField] private EnemyListen listen;
    [SerializeField] private EnemyRoaming roam;
    [SerializeField] private EnemyPathfinding pathfind;
    [SerializeField] private EnemyTransformer transformer;

    private GameObject player;
    private Vector3 lastKnownPos;
    private float waitTimer;

    private Vector3 dir;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) Debug.LogError("Missing Player Object!");

        IsChasing = false;
        IsRoaming = true;
    }

    void FixedUpdate()
    {
        // Vision and Listen sense
        vision.UpdateVision(player.transform);
        listen.UpdateListen();

        // Chase if vision or listen are activated
        if (vision.CanSeeTarget || listen.HearSound)
        {
            IsChasing = true;
            IsRoaming = false;
            waitTimer = 0f;

            lastKnownPos = player.transform.position;
        }
        else if (Vector3.Distance(transform.position, lastKnownPos) <= data.chaseSpeed)
        {
            IsChasing = false;
        }

        if (IsChasing) // Enemy is chasing
        {
            transformer.SetSpeed(data.chaseSpeed);

            if (vision.CanSeeTarget)
            {
                dir = vision.TargetDir;
            }
            else
            {
                pathfind.UpdatePath(lastKnownPos);
                pathfind.SetTargetNodeTransforms();
                dir = pathfind.TargetDir;
            }
        }
        else if (IsRoaming) // Enemy is roaming
        {
            transformer.SetSpeed(data.roamingSpeed);
            roam.UpdateMovement();
        }
        else // Enemy is waiting or returning to roaming location
        {
            if (waitTimer >= 3.0f) // Return back to roaming location
            {
                transformer.SetSpeed(data.roamingSpeed);

                pathfind.UpdatePath(roam.nodes[roam.AtNode].position);
                pathfind.SetTargetNodeTransforms();
                dir = pathfind.TargetDir;


            }
            else // Wait and add to timer
            {
                waitTimer += Time.fixedDeltaTime;
                dir = new Vector3(0, 0, 0);
            }
            
        }

        // Roaming handles transforms already
        if (IsRoaming) return;

        // Transform Enemy
        transformer.UpdateMovement(dir);
        transformer.UpdateDirection(dir);

        // Check any changes for pathfinding after transform
        pathfind.SetNextTargetNode();

        // If at roaming location, start roaming
        if (!IsChasing && Vector3.Distance(transform.position, roam.nodes[roam.AtNode].position) < 1) 
            IsRoaming = true;
    }
}

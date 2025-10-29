using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public bool IsChasing { get; private set; }
    public bool IsRoaming { get; private set; }

    [SerializeField] private EnemyData data;
    [SerializeField] private EnemyTransformer transformer;
    [SerializeField] private EnemyVision vision;
    [SerializeField] private EnemyPathfinding pathfind;
    [SerializeField] private EnemyRoaming roam;

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

        //lastKnownPos = new Vector3(5.6f, 0f, 0f);
    }

    void FixedUpdate()
    {
        vision.UpdateVision(player.transform);

        if (vision.CanSeeTarget /* || "hears sound" */)
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

        Debug.Log("position: " + transform.position + "; lastKnownPos: "+ lastKnownPos);

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
                pathfind.SetTargetNodeTransforms();
                dir = pathfind.TargetDir;
            }
        }
        else if (IsRoaming)
        {
            transformer.SetSpeed(data.roamingSpeed);
            roam.UpdateMovement();
        }
        else // wait then return to roaming location
        {
            if (waitTimer >= 3.0f)
            {
                transformer.SetSpeed(data.roamingSpeed);

                pathfind.UpdatePath(roam.nodes[0].position);
                pathfind.SetTargetNodeTransforms();
                dir = pathfind.TargetDir;
            }
            else
            {
                waitTimer += Time.fixedDeltaTime;
                dir = new Vector3(0, 0, 0);
            }
            
        }

        if (IsRoaming) return; // TEMPORARY SOLUTION: Roaming handles transforms already

        // Transform Enemy
        transformer.UpdateMovement(dir);
        transformer.UpdateDirection(dir);

        // Check any changes after transform
        pathfind.SetNextTargetNode();

        if (!IsChasing && Vector3.Distance(transform.position, roam.nodes[0].position) < 1) IsRoaming = true;
    }
}

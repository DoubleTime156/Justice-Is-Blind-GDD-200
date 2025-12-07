using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
    [SerializeField] private PersonAnimator _personAnimator;

    private bool isAlert;

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

        isAlert = false;
    }


    void FixedUpdate()
    {
        // Vision and Listen sense
        vision.UpdateVision(player.transform.position);

        // Chase if vision or listen are activated
        if (vision.CanSeeTarget || listen.HearSound)
        {

            isAlert = true;
            IsRoaming = false;
            waitTimer = 3f;

            // Wait for 0.75 seconds then chase
            StartCoroutine(StartChase(0.85f));

        }
        else if (Vector3.Distance(transform.position, lastKnownPos) <= data.chaseSpeed)
        {
            IsChasing = false;
            isAlert = false;
        }


        if (IsChasing) // Enemy is chasing
        {
            transformer.SetSpeed(data.chaseSpeed);
            vision.UpdateVision(lastKnownPos);
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
            if (roam.RoamType != "path") _personAnimator.movement = new Vector2(0, 0);
            else _personAnimator.movement = roam.newDir;
        }
        else if (!isAlert) // Enemy is waiting or returning to roaming location
        {
            if (waitTimer <= 0)
            {
                waitTimer = 0;

                transformer.SetSpeed(data.roamingSpeed);

                pathfind.UpdatePath(roam.Nodes[roam.AtNode].position);
                pathfind.SetTargetNodeTransforms();
                dir = pathfind.TargetDir;
            }
            else
            {
                waitTimer -= Time.fixedDeltaTime;
                dir = new Vector3(0, 0, 0);
            }
        }
        else // Enemy is looking at player and doesn't move
        {
            transformer.UpdateDirection(dir);
            _personAnimator.movement = new Vector2(0, 0);
            return;
        }

        // Roaming handles transforms already
        if (IsRoaming) return;

        // Transform Enemy
        transformer.UpdateMovement(dir);
        transformer.UpdateDirection(dir);

        _personAnimator.movement = dir;

        // Check any changes for pathfinding after transform
        pathfind.SetNextTargetNode();

        // If at roaming location, start roaming
        if (!IsChasing && Vector3.Distance(transform.position, roam.Nodes[roam.AtNode].position) < 1)
            IsRoaming = true;
    }


    IEnumerator StartChase(float delay)
    {
        bool sawTarget = vision.CanSeeTarget;

        // Set lastKnownPos
        if (sawTarget)
        {
            lastKnownPos = player.transform.position;
        }
        else
        {
            if (listen.ObjectEmitterTransform != null)
            {
                Vector3 listenPos = listen.ObjectEmitterTransform.position;
                lastKnownPos = listenPos;
            }
            listen.SetHearSound(false);
        }

        // Look at sound
        vision.UpdateVision(lastKnownPos);
        dir = vision.TargetDir;
        transformer.UpdateDirection(dir);

        yield return new WaitForSeconds(delay);

        // Update lastKnownPos after delay if saw target
        if (sawTarget && !IsChasing)
            lastKnownPos = player.transform.position;

        IsChasing = true;
    }


    /*
    IEnumerator StartWalkBack(float delay)
    {
        isWaiting = true;
        yield return new WaitForSeconds(delay);
        if (IsChasing || IsRoaming) yield break;

        transformer.SetSpeed(data.roamingSpeed);

        pathfind.UpdatePath(roam.Nodes[roam.AtNode].position);
        pathfind.SetTargetNodeTransforms();
        dir = pathfind.TargetDir;

        isWaiting = false;
    }
    */
}

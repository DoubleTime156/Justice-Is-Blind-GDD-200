using System;
using UnityEngine;

public class EnemyFootsteps : MonoBehaviour
{
    private Rigidbody2D rb;
    public float soundInterval = 0.75f;
    private float intervalTimer = 0f;
    private Vector3 lastPosition;
    public ParticleSystem footstepParticles;
    public float posThreshhold = 0.001f;
    public float moveToFeet = -0.92f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
    }
    bool isMoving()
    {

        //grab enemys movement if they arent moving it returns false, if the movement is over 0.001f its true
        Vector3 delta = transform.position;
        

        if (delta != lastPosition)
        {
            //Debug.Log("moving");
            return true;
        }
        else
        {
            //Debug.Log("not moving");
            return false;
        }

    }


    void SpawnFootstep()
    {
        if (footstepParticles != null) {
            Vector3 spawnPos = transform.position;
            spawnPos.y += moveToFeet;

            ParticleSystem ps = Instantiate(footstepParticles, spawnPos, Quaternion.identity);

            ps.Play();

            var main = ps.main;
            float life = main.duration + main.startLifetime.constantMax;
            Destroy(ps.gameObject, life);

        }
    }


    // Update is called once per frame
    void Update()
    {

        bool moving = isMoving();
        if (!moving)
        {
            //no move no particle
            intervalTimer = 0f;
            return;
        }
        intervalTimer += Time.deltaTime;

        if (intervalTimer >= soundInterval) { 
            
            SpawnFootstep();
            intervalTimer = 0f;
            lastPosition = transform.position;

        }
    }
}

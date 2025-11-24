using System;
using UnityEngine;

public class HideEnemies : MonoBehaviour
{
    public float viewRadius = 5f;
    public Transform player;
    public float multipler = 2f;
    public float timer = 1f;

    private float time = 0f;
    private Boolean pressed = false;
    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (Input.GetKeyDown(KeyCode.Space) && !pressed)
        {
            pressed = true;
            time = timer;
        }

        if (pressed == true)
        {
            time -= Time.deltaTime;
        }

        if (time <= 0 && pressed == true)
        {
            pressed = false;
            viewRadius = 5;

        }
        
        if (pressed)
        {
            viewRadius = 10;
        }

        foreach (GameObject enemy in enemies)
        {
            Renderer enemyRenderer = enemy.GetComponent<Renderer>();
            if (enemyRenderer != null)
            {
                float distance = Vector3.Distance(player.position, enemy.transform.position);

                enemyRenderer.enabled = distance <= viewRadius;

            }
            //Light lightRenderer = enemy.GetComponentInChildren<Light>();
            //if (enemyRenderer != null)
            //{
            //    float distance = Vector3.Distance(player.position, enemy.transform.position);

            //    lightRenderer.enabled = distance <= viewRadius;

            //}
        }
    }
}

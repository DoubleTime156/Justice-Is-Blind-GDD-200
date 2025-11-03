using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    public AudioClip calm;

    [SerializeField]
    public AudioClip chase;

    [SerializeField]
    public AudioSource audioSource;

    private GameObject[] enemyInTrigger = GameObject.FindGameObjectsWithTag("Enemy");

    private bool allEnemiesCalm = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if any enemy is chasing, play chase music
        foreach (GameObject enemy in enemyInTrigger)
        {
            if (!enemy.GetComponent<EnemyAI>().IsRoaming)
            {
                toggleToChase();
                //track if any enemies are chasing
                allEnemiesCalm = false;
            }
        }
        
        //if the danger sensor isn't triggered, play calm music
        if (allEnemiesCalm)
        {
            toggleToCalm();
        }

        //resets the danger sensor
        allEnemiesCalm = true;

    }

    private void toggleToChase()
    {
        //if audiosource is not playing chase, play chase
        //if audiosource is already playing chase, don't do anything
        if (audioSource.clip != chase || !audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = chase;
            audioSource.Play();
        }
    }

    private void toggleToCalm()
    {
        //if audiosource is not playing calm, play calm
        //if audiosource is already playing calm, don't do anything
        if (audioSource.clip != calm || !audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = calm;
            audioSource.Play();
        }
        
    }

}

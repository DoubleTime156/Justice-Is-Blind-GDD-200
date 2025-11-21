using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    public AudioClip calm;

    [SerializeField]
    public AudioClip chase;

    [SerializeField]
    public AudioSource audioSource;

    private GameObject[] enemyInTrigger;

    private bool allEnemiesCalm = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyInTrigger = GameObject.FindGameObjectsWithTag("Enemy");
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyInTrigger.Length == 0)
        {
            return;
        }
        
        foreach (GameObject enemy in enemyInTrigger)
        {
            if (enemy == null) continue;

            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai == null) continue;

            if (ai.IsChasing)
            {
                toggleToChase();
                allEnemiesCalm = false;
            }
        }

        if (allEnemiesCalm)
        {
            toggleToCalm();
        }
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

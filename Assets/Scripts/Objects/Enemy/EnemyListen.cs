using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyListen : MonoBehaviour
{
    public GameObject ObjectEmitter {  get; private set; }

    public float hearDistance;

    public bool HearSound;

    private List<GameObject> activeTargets = new List<GameObject>();

    public void UpdateListen()
    {
        HearSound = false;


        // Old Listen Code
        /*
        HearSound = false;
        activeTargets.Clear();

        //ObjectSound[] emitters = FindObjectsByType<ObjectSound>(FindObjectsSortMode.None);
        SoundMaker[] emitters = FindObjectsByType<SoundMaker>(FindObjectsSortMode.None);

        foreach (SoundMaker e in emitters)
        {
            if (e.IsMakingSound && 
                Vector3.Distance(transform.position, e.gameObject.transform.position) < e.soundRadius)
            {
                activeTargets.Add(e.gameObject);
                HearSound = true;
            }
        }
        float shortestDistance = 1000f;
        for (int i = 0; i < activeTargets.Count; i++) // loop through activeTargets and compare distance
        {
            float distance = Vector3.Distance(activeTargets[i].transform.position, transform.position);
            // set shortest activeTarget to ObjectEmitter
            if (distance<shortestDistance)
            {
                shortestDistance = distance;
                ObjectEmitter = activeTargets[i];
            }
        }
        */
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Enemy hears throwable");
        if (other.CompareTag("Sound"))
        {
            HearSound = true;
            ObjectEmitter = other.GetComponent<GameObject>();
        }
    }


    /*
    void OnCollisionEnter2D(PolygonCollider2D poly)
    {
        if (poly.CompareTag("Sound"))
        {
            HearSound = true;
            ObjectEmitter = poly.GetComponent<GameObject>();
        }
    }
    */
}

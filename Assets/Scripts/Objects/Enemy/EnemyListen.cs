using System.Collections.Generic;
using UnityEngine;

public class EnemyListen : MonoBehaviour
{
    public GameObject ObjectEmitter {  get; private set; }

    public float hearDistance;

    public bool HearSound { get; private set; }

    private List<GameObject> activeTargets = new List<GameObject>();

    public void UpdateListen()
    {
        HearSound = false;
        activeTargets.Clear();

        ObjectSound[] emitters = FindObjectsByType<ObjectSound>(FindObjectsSortMode.None);

        foreach (ObjectSound e in emitters)
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
    }
}

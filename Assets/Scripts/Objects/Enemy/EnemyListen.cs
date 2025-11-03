using System.Collections.Generic;
using UnityEngine;

public class EnemyListen : MonoBehaviour
{

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
    }
}

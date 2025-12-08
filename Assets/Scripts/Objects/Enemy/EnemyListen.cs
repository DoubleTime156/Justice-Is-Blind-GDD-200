using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyListen : MonoBehaviour
{
    public Transform ObjectEmitterTransform {  get; private set; }

    public float hearDistance;

    public bool HearSound { get; private set; }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(Time.time);
        if (other.CompareTag("Sound"))
        {
            HearSound = true;
            ObjectEmitterTransform = other.GetComponent<Transform>();
        }
    }

    public void SetHearSound(bool newHearSound)
    {
        HearSound = newHearSound;
    }
}

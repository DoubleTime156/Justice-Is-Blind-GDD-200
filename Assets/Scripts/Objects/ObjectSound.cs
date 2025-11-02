using System;
using UnityEngine;

public class ObjectSound : MonoBehaviour
{
    public bool IsMakingSound { get; set; }
    public float soundRadius;

    void Awake()
    {
        IsMakingSound = false;
    }

    // Debug - Sound Circle
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }
}

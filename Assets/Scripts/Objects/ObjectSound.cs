using System;
using UnityEngine;

public class ObjectSound : MonoBehaviour
{
    public bool IsMakingSound { get; private set; }
    public float soundRadius;

    void Awake()
    {
        IsMakingSound = true;
    }

    // Debug - Sound Circle
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }
}

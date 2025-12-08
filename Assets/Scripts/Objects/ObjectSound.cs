using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using UnityEngine;
using Object = System.Object;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ObjectSound : MonoBehaviour
{
    public bool IsMakingSound { get; set; }
    public float soundRadius;

    [SerializeField] GameObject soundMaker;

    void Awake()
    {
        IsMakingSound = false;
    }

    void FixedUpdate()
    {
        if (IsMakingSound)
        {
            IsMakingSound = false;
            soundMaker.transform.position = gameObject.transform.position;
            Instantiate(soundMaker);
        }
    }

    // Debug - Sound Circle
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }
}
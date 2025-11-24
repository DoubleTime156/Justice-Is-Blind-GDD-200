using System;
using System.Collections.Generic;
using System.Numerics;
using Mono.Cecil.Cil;
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

    private float tempTimer = 0f;

    void FixedUpdate()
    {
        if (IsMakingSound && tempTimer <= 0)
        {
            IsMakingSound = false;
            tempTimer = 3f;
            Instantiate(soundMaker, transform);
        }

        tempTimer -= Time.fixedDeltaTime;
    }

    // Debug - Sound Circle
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blueViolet;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }
}
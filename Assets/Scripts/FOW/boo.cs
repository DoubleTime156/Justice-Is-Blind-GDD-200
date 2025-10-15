using System.Collections;
using UnityEngine;

public class boo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(HihiEnumerator());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator HihiEnumerator()
    {
        yield return new WaitForSeconds(5);

        print("yo");
    }
}

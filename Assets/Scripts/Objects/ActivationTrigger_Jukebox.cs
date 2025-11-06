using UnityEngine;

public class ActivationTrigger_Jukebox : MonoBehaviour, BehaviorCallable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Behavior()
    {
        //this is where to put the behavior of the object when interacted with
        Debug.Log("Hey! I, the JukeBox, have been activated.");
    }
}
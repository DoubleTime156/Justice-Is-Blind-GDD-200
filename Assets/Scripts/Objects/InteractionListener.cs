using System.Collections.Generic;
using UnityEngine;
public interface BehaviorCallable
{
    void Behavior();
}
public class InteractionListener : MonoBehaviour
{
    private GameObject thisObj;

    [SerializeField]
    private CircleCollider2D triggerRadius;

    private GameObject[] overlapArray;

    private List<GameObject> interactableInTrigger = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisObj = gameObject;
}

    // Update is called once per frame
    void Update()
    {
        //if interaction is pressed down
        //  get all objects inside interactable in trigger and then do their stuff
        /*if (Interact.performed)
        {
            foreach (GameObject obj in interactableInTrigger)
            {
                //tk: needs to call the behavior of the script
                //tk: behavior of the script needs to toggle (not for all but for most)
                obj.GetComponent<Collider>().enabled = false;
            }
        }*/
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            interactableInTrigger.Add(other.gameObject);
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            interactableInTrigger.Remove(other.gameObject);
        }
    }

}

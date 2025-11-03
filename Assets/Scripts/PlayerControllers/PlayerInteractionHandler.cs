using UnityEditor.Experimental.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInteractionHandler : MonoBehaviour
{
    //initializes a the player object so we don't have to find it later
    private GameObject playerObj = null;
    private GameObject targetObj = null;
    private bool keyStillDown = false;
    private float callInterval = 0.1f;
    private float timer;
    private BehaviorCallable targetScript = null;
  
    void Start()
    {
        //fills the playerObj with the propper data
        playerObj = GameObject.Find("Player");
        targetObj = ListClosest();
        timer = callInterval;
    }

    // Update is called once per frame
    void Update()
    {
        //list closest interactable object 10 times a second
        //find the distance
        //if distance is less than x and e is pressed, then do the thing
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            targetObj = ListClosest();
            targetScript = targetObj.GetComponent<BehaviorCallable>();

            // Reset the timer, carrying over any extra time to maintain precision
            timer += callInterval;


        }

        //performs the interaction if e is pressed
        if (/*Input.GetButtonDown("e") &&*/ !keyStillDown /* && distance <= distanceVariable*/)
        {
            targetObj.GetComponent<BehaviorCallable>().Behavior();
            keyStillDown = true;
        }
        //can only interact once per button press
        /*
        if (!Input.GetButtonDown("e"))
        {
            keyStillDown = false;
        }*/
    }

    GameObject ListClosest()
    {
        //get an array of all objects with a specific tag
        GameObject[] actObj = GameObject.FindGameObjectsWithTag("interactable");

        //instantiate closest object var for comparison reasons
        GameObject closestObject = actObj[0];
        //distance comparison number between obj 0 and player
        float lastObjDist = (playerObj.transform.position - closestObject.transform.position).sqrMagnitude;
        float currentObjDist = lastObjDist;

        //checks for closest
        foreach (GameObject obj in actObj)
        {
            currentObjDist = (playerObj.transform.position - obj.transform.position).sqrMagnitude;
            if (lastObjDist <= currentObjDist)
            {
                closestObject = obj;
            }
            lastObjDist = currentObjDist;
        }

        return closestObject;
    }
}

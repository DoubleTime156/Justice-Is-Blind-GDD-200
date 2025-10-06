using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrow : MonoBehaviour
{
    private float[] speed = {20,15};
    public GameObject Coin;
    public GameObject Bottle;
    private int heldItem = 0; // (0 - coin, 1 - bottle)
    private float cooldownTime = 1; // How long to wait before item can be thrown
    private float nextFireTime = 0;
    public bool isCoolingDown => Time.time > nextFireTime; // Checks if the current time has passed cooldown time (if StartCooldown is executed)


    // Player presses F to throw object
    public void OnThrow(InputAction.CallbackContext context)
    {
        if (isCoolingDown) {
            Debug.Log("Throw object");

            if (context.canceled) { // cancelled - after the key is let go
                switch (heldItem)
                {
                    case 0: // Coin
                        // Spawn at player’s position
                        GameObject thrownObj = Instantiate(Coin, transform.position, Quaternion.identity);

                        // Get mouse position
                        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
                        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
                        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

                        // Initialize the object’s movement
                        thrownObj.GetComponent<Throwable>().Init(mouseWorldPos, speed[0], heldItem);
                        StartCooldown();
                        break;
                    case 1: // Bottle
                            // Spawn at player’s position
                        thrownObj = Instantiate(Bottle, transform.position, Quaternion.identity);

                        // Get mouse position
                        mouseScreenPos = Mouse.current.position.ReadValue();
                        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
                        mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

                        // Initialize the object’s movement
                        thrownObj.GetComponent<Throwable>().Init(mouseWorldPos, speed[1], heldItem);
                        StartCooldown();
                        break;
                }
            }
        }
    }

    // Press Q to swap throwable

    public void OnSwap(InputAction.CallbackContext context)
    {
        if (context.canceled) 
        {
            switch (heldItem)
            {
                case 0:
                    heldItem = 1;
                    Debug.Log("Holding: Bottle");
                    break;
                case 1:
                    heldItem = 0;
                    Debug.Log("Holding: Coin");
                    break;
            }
        }
    }

    // Cooldown for throwable items
    public void StartCooldown() => nextFireTime = Time.time + cooldownTime;

}

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrow : MonoBehaviour
{
    private float[] speed = {30,60};
    public GameObject[] items;
    private int heldItem = 0; // (0 - coin, 1 - bottle)
    private float cooldownTime = 1; // How long to wait before item can be thrown
    private float nextFireTime = 0;
    public bool isCoolingDown => Time.time > nextFireTime; // Checks if the current time has passed cooldown time (if StartCooldown is executed)
    private PlayerController2D_InputSystem playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController2D_InputSystem>();
    }


    // Player presses F to throw object
    public void OnThrow(InputAction.CallbackContext context)
    {
        if (isCoolingDown) {
            Debug.Log("Throw object");

            if (context.canceled)// cancelled - after the key is let go
            {
                if (playerController.inventory[heldItem] > 0)
                {
                    throwItem();
                }
            }
        }
    }

    public void throwItem()
    {
        // Spawn at player’s position
        GameObject thrownObj = Instantiate(items[heldItem], transform.position, Quaternion.identity);

        // Get mouse position
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // Initialize the object’s movement
        thrownObj.GetComponent<Throwable>().Init(mouseWorldPos, speed[heldItem], heldItem);
        playerController.inventory[heldItem]--;
        StartCooldown();
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

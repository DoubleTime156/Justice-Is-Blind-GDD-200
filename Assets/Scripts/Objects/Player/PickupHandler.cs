using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class PickupHandler : MonoBehaviour
{
    private List<Pickup> nearbyPickups = new List<Pickup>();
    private PlayerController2D_InputSystem playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController2D_InputSystem>();
    }

    // Pick up item when interacted
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            if (nearbyPickups.Count > 0)
            {
                Pickup pickup = nearbyPickups[0];
                if (pickup != null)
                {
                    pickup.Collect(playerController);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Pickup pickup = collision.GetComponent<Pickup>();
        if (pickup != null && !nearbyPickups.Contains(pickup)) 
        {
            nearbyPickups.Add(pickup);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Pickup pickup = collision.GetComponent<Pickup>();
        if (pickup != null && nearbyPickups.Contains(pickup))
        {
            nearbyPickups.Remove(pickup);
        }
    }

}

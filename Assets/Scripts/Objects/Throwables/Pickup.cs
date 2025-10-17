using UnityEngine;

public class Pickup: MonoBehaviour
{
    public int pickupType;
    public string itemName = "Pickup";

    // Call this when the player interacts with pickup
    public void Collect(PlayerController2D_InputSystem player)
    {
        Debug.Log($"Picked up {itemName}");
        player.inventory[pickupType]++; // Add 1 item to player inventory
        Debug.Log(itemName + ": " + player.inventory[pickupType].ToString());
        Destroy(gameObject);
    }
}

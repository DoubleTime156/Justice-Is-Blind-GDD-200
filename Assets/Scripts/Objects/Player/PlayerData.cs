using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public float moveSpeed = 0.15f;
    public int heldItem = 1; // (0 - coin, 1 - bottle)
    public int[] inventory = { 0, 0 }; //{Coins held, Bottles Held}
    public bool hasKey = false;
}

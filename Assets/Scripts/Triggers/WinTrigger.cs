using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinTrigger2D : MonoBehaviour
{
    public GameObject winText; // Drag your UI Text or TextMeshPro object here

    void Start()
    {
        if (winText != null)
            winText.SetActive(false); // Hide text at start
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Triggered with: " + other.name);

        if (other.CompareTag("Player"))
        {
            if (winText != null)
                winText.SetActive(true); // Show the "You Win" text
        }
    }
}

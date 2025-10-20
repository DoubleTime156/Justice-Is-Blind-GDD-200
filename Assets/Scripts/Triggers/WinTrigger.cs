using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinTrigger2D : MonoBehaviour
{
    public GameObject winText; 

    void Start()
    {
        if (winText != null)
            winText.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Triggered Win");

        if (other.CompareTag("Player"))
        {
            if (winText != null)
                winText.SetActive(true);
        }
    }
}

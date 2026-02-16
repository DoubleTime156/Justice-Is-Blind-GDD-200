using TMPro;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public string displayText = "Sample text";
    public TextMeshProUGUI textObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Enable and set text
            textObject.text = displayText;
            textObject.enabled = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        // Check for player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Disable text
            textObject.enabled = false;
        }
    }
}

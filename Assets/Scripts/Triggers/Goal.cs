using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    public string levelName; // Name of the level file to load
    public bool keyRequired = true; // If true, this goal does nothing if the player doesn't have a key


    // Runs when something enters this trigger
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for player and key, if applicable
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController2D_InputSystem playerController = collision.gameObject.GetComponent<PlayerController2D_InputSystem>();
            PlayerData playerData = playerController.data;
            if (!keyRequired || playerData.hasKey)
            {
                // Use key
                playerData.hasKey = false;

                // Switch scene to the next level
                SceneManager.LoadSceneAsync(levelName);
            }
        }
    }
}

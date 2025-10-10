using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    public string levelName; // Name of the level file to load



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }



    // Update is called once per frame
    void Update()
    {
        
    }



    // Runs when something enters this trigger
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // If the colliding object is a player, switch scenes
        if (collision.gameObject.CompareTag("Player"))
        {
            // Stop anything that needs to be stopped, show results screen,
            // whatever we plan to do BEFORE scene is switched

            // Switch scene to the next level
            SceneManager.LoadSceneAsync(levelName);
        }
    }
}

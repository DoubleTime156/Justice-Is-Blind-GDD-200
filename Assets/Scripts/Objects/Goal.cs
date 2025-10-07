using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    private BoxCollider2D goalCollider; // Collider of the associated game object, assigned on Start()

    public string levelName; // Name of the level file to load



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Assign components
        goalCollider = GetComponent<BoxCollider2D>();

    }



    // Update is called once per frame
    void Update()
    {
        
    }



    // Runs when something enters this trigger
    private void OnCollisionEnter2D(Collision2D other)
    {
        // If the colliding object is a player, switch scenes
        if (other.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadSceneAsync(levelName);
        }
    }
}

using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PlayerData data;

    public void startGame() 
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_Tutorial");
    }

    public void quitGame()
    {
        Time.timeScale = 1;
        Debug.Log("Exiting Game");
        Application.Quit();

    }
    public void restartGame()
    {
        Debug.Log("restart");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }

    public void mainMenu()
    {
        Debug.Log("Quit to menu");
        Time.timeScale = 1; 
        SceneManager.LoadScene("Main Menu");
        
    }
   
}

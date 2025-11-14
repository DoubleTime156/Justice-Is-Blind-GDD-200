using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PlayerData data;

    public TextMeshProUGUI gameOverText;
    public Button restartButton;


    public void startGame() // Start game from beginning when Start is pressed on main menu
    {
        SceneManager.LoadScene("Level_Tutorial");
    }

    public void quitGame()
    {
        Application.Quit();
        Debug.Log("Exiting Game");
    }
    public void gameOver()
    {
        Debug.Log("Game Over");
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }
    public void restartGame() // Reload the scene when restart is clicked
    {
        Debug.Log("restart");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
   
}

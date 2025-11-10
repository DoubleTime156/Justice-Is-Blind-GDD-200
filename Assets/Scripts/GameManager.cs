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
    public GameObject keyImage;
    public TextMeshProUGUI bottleText;
    public TextMeshProUGUI coinText;


    public void Start() // Set player inventory to zero on scene load
    {
        data.inventory[0] = 0;
        data.inventory[1] = 0;
        data.heldItem = 0;
        data.hasKey = false;
    }

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
    public void updateAmount()
    {
        bottleText.text = data.inventory[1].ToString();
        coinText.text = data.inventory[0].ToString();
        if (data.hasKey) { keyImage.SetActive(true); }
    }
}

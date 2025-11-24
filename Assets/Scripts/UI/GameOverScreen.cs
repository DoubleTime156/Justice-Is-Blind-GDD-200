using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button quitButton;
    public void gameOver()
    {
        Debug.Log("Game Over");
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }
}

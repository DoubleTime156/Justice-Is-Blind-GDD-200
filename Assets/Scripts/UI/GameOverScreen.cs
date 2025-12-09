using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameOver : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button quitButton;
    public static bool IsGameOver { get; set; }
    public void gameOver()
    {
        IsGameOver = true;
        Debug.Log("Game Over");
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
        Time.timeScale = 0f;

        PlayerInput pi = FindFirstObjectByType<PlayerInput>();
        if (pi != null)
        {
            pi.enabled = false;
        }

        MusicManager mm = FindFirstObjectByType<MusicManager>();
        if (mm != null)
        {
            mm.OnGameOver();
        }
    }
}

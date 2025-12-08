using UnityEngine;

public class PauseGame : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject pauseButton;
    public bool isPaused = false;
    public void pauseGame()
    {
        pauseMenu.SetActive(true);
        pauseButton.SetActive(false);
        Time.timeScale = 0;
        isPaused = true;
    }

    public void unpauseGame()
    {
        pauseButton.SetActive(true);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }
}

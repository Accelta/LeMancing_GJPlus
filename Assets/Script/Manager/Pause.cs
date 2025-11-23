using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseUI;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ClosePause();
            else OpenPause();
        }
    }

    public void OpenPause()
    {
        pauseUI.SetActive(true);
        Time.timeScale = 0f;  // Freeze EVERYTHING using deltaTime
        isPaused = true;
    }

    public void ClosePause()
    {
        pauseUI.SetActive(false);
        Time.timeScale = 1f;  // Unfreeze
        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}

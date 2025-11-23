using UnityEngine;
using UnityEngine.SceneManagement; // For loading scenes

public class MenuManager : MonoBehaviour
{
    // Called when the Play button is clicked
    public void PlayGame()
    {
        // Replace "GameScene" with the name of your game scene
        SceneManager.LoadScene("Gameplay");
    }

    // Called when the Exit button is clicked
    public void ExitGame()
    {
        #if UNITY_EDITOR
        // Exits the application
        Debug.Log("Exiting game...");
        #endif
        Application.Quit();
    }
}

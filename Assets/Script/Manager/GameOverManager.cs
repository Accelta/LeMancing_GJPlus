using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel; // root panel (set inactive by default in Inspector)

    [Header("Left Column")]
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI currentWaveText;

    [Header("Right Column")]
    public TextMeshProUGUI highestScoreText;
    public TextMeshProUGUI highestWaveText;

    [Header("Optional")]
    public GameObject restartButton;
    public Animator panelAnimator; // optional animator (Show/Hide triggers)

    [Header("PlayerPrefs keys")]
    public string highScoreKey = "HighScore";
    public string highWaveKey = "HighWave";

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowGameOver(int finalScore, int finalWave)
    {
        Debug.Log($"GameOverUI: ShowGameOver called (score={finalScore}, wave={finalWave})");

        if (panel == null)
        {
            Debug.LogError("GameOverUI: panel reference is NULL! Assign it in the inspector.");
            return;
        }

        // Defensive: try to force visibility in case animator/canvasgroup/scale hides it
        ForceShowPanel();

        // Load and update highs
        int storedHighScore = PlayerPrefs.GetInt(highScoreKey, 0);
        int storedHighWave = PlayerPrefs.GetInt(highWaveKey, 1);

        if (finalScore > storedHighScore)
        {
            storedHighScore = finalScore;
            PlayerPrefs.SetInt(highScoreKey, storedHighScore);
        }

        if (finalWave > storedHighWave)
        {
            storedHighWave = finalWave;
            PlayerPrefs.SetInt(highWaveKey, storedHighWave);
        }

        PlayerPrefs.Save();

        // Fill UI fields (guard against null)
        if (currentScoreText != null) currentScoreText.text = "CURRENT SCORE\n\n" + finalScore;
        if (currentWaveText != null)  currentWaveText.text  = "CURRENT WAVE\n\n"  + finalWave;
        if (highestScoreText != null) highestScoreText.text = "HIGHEST SCORE\n\n" + storedHighScore;
        if (highestWaveText != null)  highestWaveText.text  = "HIGHEST WAVE\n\n"  + storedHighWave;

        // Activate & animate
        panel.SetActive(true);
        if (panelAnimator != null)
        {
            panelAnimator.ResetTrigger("Hide");
            panelAnimator.SetTrigger("Show");
        }

        if (restartButton != null)
            restartButton.SetActive(true);
    }

public void RestartGame()
{
    Debug.Log("GameOverUI: RestartGame called.");
    // Ensure time is running (in case you paused on game over)
    Time.timeScale = 1f;

    // Optional: reset any static singletons or data here, e.g. GameManager.Instance = null;
    // If your GameManager stores persistent state in a DontDestroyOnLoad object, handle clearing here.

    // Reload current scene
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}

    public void Hide()
    {
        if (panelAnimator != null)
        {
            panelAnimator.ResetTrigger("Show");
            panelAnimator.SetTrigger("Hide");
        }
        else
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    /// <summary>
    /// Defensive "make visible" routine for animator/canvasgroup/scale issues.
    /// Call at start of ShowGameOver to force panel visible during debugging or in weird states.
    /// </summary>
    private void ForceShowPanel()
    {
        if (panel == null) return;

        // Enable object
        panel.SetActive(true);

        // Enable any child Canvas
        Canvas childCanvas = panel.GetComponentInChildren<Canvas>(true);
        if (childCanvas != null) childCanvas.enabled = true;

        // Fix CanvasGroup if present
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        // Reset RectTransform scale/rotation/position
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            rt.anchoredPosition3D = Vector3.zero;
        }

        // If there's an animator, temporarily disable it to stop immediate "Hide" animations
        if (panelAnimator != null && panelAnimator.enabled)
        {
            panelAnimator.enabled = false;
            // re-enable next frame (safeguard)
            StartCoroutine(ReenableAnimatorNextFrame());
        }
    }

    private System.Collections.IEnumerator ReenableAnimatorNextFrame()
    {
        yield return null;
        if (panelAnimator != null) panelAnimator.enabled = true;
    }
}

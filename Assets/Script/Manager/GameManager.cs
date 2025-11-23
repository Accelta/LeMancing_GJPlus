using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    // public int startingHealth = 3;
    // public int currentHealth;
    public int currentScore;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;

    [Header("Wave System Settings")]
    public int startingWave = 1;
    public float waveDuration = 60f;          // seconds per wave
    public int baseWaveTargetScore = 100;     // objective for wave 1
     public float waveScoreMultiplier = 1.5f;

    [Header("Wave UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI objectiveText;

    [Header("Difficulty")]
    [Tooltip("How much fish speed scales per wave (1 = no change, 1.2 = +20% per wave)")]
    public float fishSpeedMultiplierPerWave = 1.2f;

    // runtime wave data
    private int currentWave;
    private int currentWaveTargetScore;
    private float waveTimer;
    private int waveScore;          // score earned in THIS wave only
    private bool isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // currentHealth = startingHealth;
        currentScore = 0;
        isGameOver = false;

        StartWave(startingWave);
        UpdateUI();
    }

    private void Update()
    {
        if (isGameOver) return;

        UpdateWaveTimer();
    }

    // =========================
    // WAVE SYSTEM
    // =========================
private void StartWave(int waveIndex)
{
    currentWave = waveIndex;
    waveTimer = waveDuration;
    waveScore = 0;

    // Multiplicative wave scaling
    currentWaveTargetScore = Mathf.CeilToInt(
        baseWaveTargetScore * Mathf.Pow(waveScoreMultiplier, currentWave - 1)
    );

    UpdateUI();
}

    private void UpdateWaveTimer()
    {
        waveTimer -= Time.deltaTime;
        if (waveTimer < 0f)
            waveTimer = 0f;

        UpdateWaveUI();

        // Time up: check if player met objective
        if (waveTimer <= 0f)
        {
            if (waveScore >= currentWaveTargetScore)
            {
                // Wave cleared just in time
                CompleteCurrentWave();
            }
            else
            {
                // Failed to reach score objective
                WaveFailed();
            }
        }
    }

    private void CompleteCurrentWave()
    {
        // Optional: reward, heal, bonus, etc.
        // SoundManager.PlaySFX("WaveClear");

        // Next wave
        StartWave(currentWave + 1);
    }

    private void WaveFailed()
    {
        Debug.Log("Wave failed: objective not met in time.");
        GameOver();
    }

    // =========================
    // SCORE / DAMAGE
    // =========================
    public void ResolveCatch(CatchableItem item)
    {
        if (isGameOver) return;
        if (item == null || item.data == null) return;

        var data = item.data;

        if (data.isHazard)
        {
            // ApplyDamage(data.damageAmount);
        }
        else
        {
            SoundManager.PlaySFX("AddScore");
            SoundManager.PlaySFX("WaterSplash");
            AddScore(data.scoreValue);
        }
    }

    public void AddScore(int score)
    {
        if (isGameOver) return;

        currentScore += score;   // total score across game
        waveScore += score;      // score for current wave only

        UpdateUI();
        CheckWaveProgress();
    }

    private void CheckWaveProgress()
    {
        // If player reaches target before timer ends, immediately go to next wave
        if (waveScore >= currentWaveTargetScore && waveTimer > 0f)
        {
            CompleteCurrentWave();
        }
    }

    // public void ApplyDamage(int damage)
    // {
    //     if (isGameOver) return;

    //     currentHealth -= damage;
    //     if (currentHealth < 0) currentHealth = 0;

    //     UpdateUI();

    //     if (currentHealth <= 0)
    //     {
    //         GameOver();
    //     }
    // }

    private void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Game Over!");
        // TODO: show game over UI, restart, etc.
        // SoundManager.PlaySFX("GameOver");
    }

    // =========================
    // UI
    // =========================
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;

        // if (healthText != null)
        //     healthText.text = "Health: " + currentHealth;

        UpdateWaveUI();
    }

    private void UpdateWaveUI()
    {
        if (waveText != null)
            waveText.text = "Wave: " + currentWave;

        if (objectiveText != null)
            objectiveText.text =
                "Target: " + waveScore + " / " + currentWaveTargetScore;

        if (timerText != null)
            timerText.text = FormatTime(waveTimer);
    }

    private string FormatTime(float time)
    {
        int t = Mathf.Max(0, Mathf.CeilToInt(time));
        int minutes = t / 60;
        int seconds = t % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    public float GetFishSpeedMultiplier()
{
    // If you start at wave 1, we want 1x on wave 1
    if (currentWave <= 1) return 1f;

    // Multiplicative growth: 1.2, 1.44, 1.73, etc if multiplier = 1.2
    return Mathf.Pow(fishSpeedMultiplierPerWave, currentWave - 1);
}
}

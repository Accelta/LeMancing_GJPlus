using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public int startingHealth = 3;
    public int currentHealth;
    public int currentScore;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;

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
        currentHealth = startingHealth;
        currentScore = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;

        if (healthText != null)
            healthText.text = "Health: " + currentHealth;
    }

    public void ResolveCatch(CatchableItem item)
    {
        if (item == null || item.data == null) return;

        var data = item.data;

        if (data.isHazard)
        {
            ApplyDamage(data.damageAmount);
        }
        else
        {
            AddScore(data.scoreValue);
        }
    }

    public void AddScore(int score)
    {
        currentScore += score;
        UpdateUI();
    }

    public void ApplyDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        // TODO: show game over UI, restart, etc.
    }
}

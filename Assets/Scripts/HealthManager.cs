using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;

    [Header("Health Settings")]
    public int maxHealth = 1000;
    public int currentHealth;
    public int damagePerMiss = 50;

    [Header("UI Elements")]
    public Image healthBarFill;
    private Coroutine flashCoroutine;
    [Header("Audio Settings")]
    public AudioSource gameOverSFX; // Assign a game over sound effect in the Inspector


    void Awake()
    {
        // Singleton pattern for easy access
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    /// <summary>
    /// Reduces the player's health by the specified damage amount.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();


        // Start flash effect
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashHealthBar());

        if (currentHealth <= 0)
        {
            // Handle game over logic here
            Debug.Log("Player has run out of health!");
            GameOver();

        }

    }

    /// <summary>
    /// Updates the health bar UI to reflect the current health.
    /// </summary>
    public void UpdateHealthBar()
    {
        float fillAmount = (float)currentHealth / maxHealth;
        healthBarFill.fillAmount = fillAmount;

        // Change color based on health percentage
        if (fillAmount > 0.5f)
        {
            healthBarFill.color = Color.blue;
        }
        else if (fillAmount > 0.25f)
        {
            healthBarFill.color = Color.cyan;
        }
        else
        {
            healthBarFill.color = Color.red;
        }
    }

    private IEnumerator FlashHealthBar()
    {
        Color originalColor = healthBarFill.color;
        Color flashColor = Color.white;
        float flashDuration = 0.1f;

        healthBarFill.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        healthBarFill.color = originalColor;
    }

    private void GameOver()
    {
        //isGameOver = true;

        // Stop the music
        if (SongManager.Instance != null)
        {
            SongManager.Instance.StopMusic();
        }

        // Play game over sound
        if (gameOverSFX != null)
        {
            gameOverSFX.Play();
        }

        // Update player stats, save data, etc.
        ScoreManager.Instance.LevelCompleted();

        // Show Game Over UI
        LevelEndUI.Instance.ShowGameOver();
    }
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        //isGameOver = false;
    }


}

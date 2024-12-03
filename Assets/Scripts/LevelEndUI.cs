using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelEndUI : MonoBehaviour
{
    public static LevelEndUI Instance;

    [Header("UI Panels")]
    public GameObject levelCompletePanel;
    public GameObject gameOverPanel;

    [Header("Level Complete UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI hitPercentageText;
    public TextMeshProUGUI missPercentageText;
    public TextMeshProUGUI earlyHitPercentageText;
    public TextMeshProUGUI perfectHitPercentageText;
    public TextMeshProUGUI lateHitPercentageText;
    public TextMeshProUGUI averageTimingText;
    public TextMeshProUGUI stageCompleteHighScoreText; // New UI element for high score

    [Header("Game Over UI Elements")]
    public TextMeshProUGUI gameOverScore, gameOverCombo;

    [Header("Buttons")]
    public Button levelCompleteMainMenuButton;
    public Button gameOverMainMenuButton;
    public Button gameOverRetryButton;

    [HideInInspector] public bool levelEnded = false;

    private bool isUIActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("LevelEndUI instance created.");
        }
        else
        {
            Debug.LogWarning("Duplicate LevelEndUI instance detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        levelCompletePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);

        Debug.Log("LevelEndUI initialized. Panels hidden.");
    }

    void Start()
    {
        // Add listeners to buttons
        if (levelCompleteMainMenuButton != null)
        {
            levelCompleteMainMenuButton.onClick.AddListener(OnMainMenuButton);
            Debug.Log("Listener added to Level Complete Main Menu Button.");
        }

        if (gameOverMainMenuButton != null)
        {
            gameOverMainMenuButton.onClick.AddListener(OnMainMenuButton);
            Debug.Log("Listener added to Game Over Main Menu Button.");
        }

        if (gameOverRetryButton != null)
        {
            gameOverRetryButton.onClick.AddListener(OnRetryButton);
            Debug.Log("Listener added to Game Over Retry Button.");
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIOpened += OnUIOpened;
            UIManager.Instance.OnUIClosed += OnUIClosed;
            Debug.Log("Subscribed to UIManager events.");
        }
        else
        {
            Debug.LogError("UIManager.Instance is null. Ensure UIManager is added to the scene.");
        }
    }

    public void ShowLevelComplete()
    {
        if (levelEnded)
        {
            Debug.LogWarning("ShowLevelComplete called but level already ended.");
            return;
        }

        levelEnded = true;
        levelCompletePanel?.SetActive(true);
        Debug.Log("Level Complete Panel shown.");

        UIManager.Instance?.OpenUIPanel();
        Time.timeScale = 0f;

        UpdateLevelCompleteUI();
    }

    public void ShowGameOver()
    {
        if (levelEnded)
        {
            Debug.LogWarning("ShowGameOver called but level already ended.");
            return;
        }

        levelEnded = true;
        gameOverPanel?.SetActive(true);
        Debug.Log("Game Over Panel shown.");

        UIManager.Instance?.OpenUIPanel();
        Time.timeScale = 0f;

        UpdateGameOverUI();
    }

    private void UpdateLevelCompleteUI()
    {
        var scoreManager = ScoreManager.Instance;
        if (scoreManager != null)
        {
            // Get the current level (scene) name
            string sceneName = SceneManager.GetActiveScene().name;

            // Get the current difficulty
            DifficultyLevel difficulty = GameSettings.Instance.difficulty;

            // Construct keys for PlayerPrefs
            string highScoreKey = $"HighScore_{sceneName}_{difficulty}";
            string levelCompletedKey = $"LevelCompleted_{sceneName}_{difficulty}";

            // Retrieve the stored high score (default to 0 if not set)
            int storedHighScore = PlayerPrefs.GetInt(highScoreKey, 0);

            // Check if the current score is higher than the stored high score
            if (scoreManager.score > storedHighScore)
            {
                // Update the high score in PlayerPrefs
                storedHighScore = (int)scoreManager.score;
                PlayerPrefs.SetInt(highScoreKey, storedHighScore);
                PlayerPrefs.Save();
            }

            // Update level completion status in PlayerPrefs
            PlayerPrefs.SetInt(levelCompletedKey, 1); // 1 indicates level completed
            PlayerPrefs.Save();

            // Update UI elements
            scoreText.text = $"Score: {scoreManager.score}";
            comboText.text = $"Highest Combo: {scoreManager.highestCombo}";
            hitPercentageText.text = $"Hit Percentage: {scoreManager.GetHitPercentage():0.00}%";
            missPercentageText.text = $"Miss Percentage: {scoreManager.GetMissPercentage():0.00}%";
            earlyHitPercentageText.text = $"Early Hits: {scoreManager.GetEarlyHitPercentage():0.00}%";
            perfectHitPercentageText.text = $"Perfect Hits: {scoreManager.GetPerfectHitPercentage():0.00}%";
            lateHitPercentageText.text = $"Late Hits: {scoreManager.GetLateHitPercentage():0.00}%";
            averageTimingText.text = $"Avg Timing Error: {scoreManager.GetAverageOffBeatDifference():0.000}s";
            stageCompleteHighScoreText.text = $"High Score: {storedHighScore}";

            Debug.Log("Level Complete UI updated with stats.");
        }
        else
        {
            Debug.LogError("ScoreManager.Instance is null. Unable to update Level Complete UI.");
        }
    }

    private void UpdateGameOverUI()
    {
        var scoreManager = ScoreManager.Instance;
        if (scoreManager != null)
        {
            gameOverScore.text = $"Score: {scoreManager.score}";
            gameOverCombo.text = $"Highest Combo: {scoreManager.highestCombo}";
            Debug.Log("Game Over UI updated with stats.");
        }
        else
        {
            Debug.LogError("ScoreManager.Instance is null. Unable to update Game Over UI.");
        }
    }

    public void OnRetryButton()
    {
        Debug.Log("Retry button clicked.");
        levelCompletePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);

        UIManager.Instance?.CloseUIPanel();

        Time.timeScale = 1f;
        ScoreManager.Instance?.ResetScore();
        HealthManager.Instance?.ResetHealth();
        Debug.Log("Score and health reset. Reloading current scene.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenuButton()
    {
        Debug.Log("Main Menu button clicked.");
        levelCompletePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);

        UIManager.Instance?.CloseUIPanel();

        // Show and unlock the cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 1f;
        Debug.Log("Loading Main Menu scene.");
        SceneManager.LoadScene("MainMenu");
    }

    private void OnUIOpened()
    {
        isUIActive = true;
        Debug.Log("UI opened.");
    }

    private void OnUIClosed()
    {
        isUIActive = false;
        Debug.Log("UI closed.");
    }

    void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIOpened -= OnUIOpened;
            UIManager.Instance.OnUIClosed -= OnUIClosed;
            Debug.Log("Unsubscribed from UIManager events.");
        }

        levelCompleteMainMenuButton?.onClick.RemoveListener(OnMainMenuButton);
        gameOverMainMenuButton?.onClick.RemoveListener(OnMainMenuButton);
        gameOverRetryButton?.onClick.RemoveListener(OnRetryButton);
        Debug.Log("Listeners removed from buttons.");
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    // Audio Sources
    public AudioSource hitSFX;
    public AudioSource missSFX;

    // UI Elements
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;

    // Scoring variables
    static int comboScore;
    public int currentCombo = 0;
    public int maxCombo = 0;
    public int highestCombo = 0;
    public int totalMisses = 0;
    public int successfulHits = 0; // Tracking hits for AI difficulty scaling - AI

    public float score = 0;
    public float scorePerNote = 10f;
    public float scorePerHoldTick = 10f; // Score per tick during hold notes
    public float comboMultiplier = 1.2f;
    public int notesHitSinceMiss = 0;   // Number of notes hit since the last miss

    // Stats tracking
    public int earlyHits = 0;
    public int perfectHits = 0;
    public int lateHits = 0;
    public List<float> offBeatDifferences = new List<float>();

    // Level information
    public string currentLevelName;
    public DifficultyLevel currentDifficulty;

    // Persistent player stats
    public PlayerStats playerStats = new PlayerStats();

    // File path for saving stats
    private string statsFileName = "playerStats.json";
    private string statsFilePath;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize AudioSources
            InitializeAudioSources();

            // Initialize stats file path
            statsFilePath = Path.Combine(Application.persistentDataPath, statsFileName);
            Debug.Log($"ScoreManager: PlayerStats JSON Path is '{statsFilePath}'");

            // Load player stats
            LoadPlayerStats();

            // Load the saved hit sound volume
            float hitSoundVolume = PlayerPrefs.GetFloat("HitSoundVolume", 1f);
            if (hitSFX != null)
            {
                hitSFX.volume = hitSoundVolume;
                Debug.Log($"ScoreManager: Hit sound volume set to {hitSoundVolume}");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetScore();
        ReassignUIReferences();

        // Set current level name based on the loaded scene
        currentLevelName = scene.name;
        Debug.Log($"ScoreManager: Current level name set to '{currentLevelName}'.");

        // Retrieve the difficulty level from PlayerPrefs
        if (PlayerPrefs.HasKey("Difficulty"))
        {
            currentDifficulty = (DifficultyLevel)PlayerPrefs.GetInt("Difficulty");
            Debug.Log($"ScoreManager: Current difficulty set to '{currentDifficulty}'.");
        }
        else
        {
            // Default to Normal if not set
            currentDifficulty = DifficultyLevel.Normal;
            Debug.LogWarning("ScoreManager: Difficulty not set in PlayerPrefs. Defaulting to 'Normal'.");
        }
    }

    void ReassignUIReferences()
    {
        // Find the new UI elements in the scene
        scoreText = GameObject.Find("Score")?.GetComponent<TextMeshProUGUI>();
        comboText = GameObject.Find("Combo")?.GetComponent<TextMeshProUGUI>();

        if (scoreText == null || comboText == null)
        {
            Debug.LogWarning("ScoreManager: UI Text elements not found in the scene.");
        }
    }

    void InitializeAudioSources()
    {
        // Assign AudioSources by finding child objects
        Transform hitSFXTransform = transform.Find("HitSFX");
        Transform missSFXTransform = transform.Find("MissSFX");

        if (hitSFXTransform != null)
        {
            hitSFX = hitSFXTransform.GetComponent<AudioSource>();
            if (hitSFX == null)
            {
                Debug.LogError("HitSFX AudioSource component missing on HitSFX GameObject.");
            }
        }
        else
        {
            Debug.LogError("HitSFX GameObject not found as a child of ScoreManager.");
        }

        if (missSFXTransform != null)
        {
            missSFX = missSFXTransform.GetComponent<AudioSource>();
            if (missSFX == null)
            {
                Debug.LogError("MissSFX AudioSource component missing on MissSFX GameObject.");
            }
        }
        else
        {
            Debug.LogError("MissSFX GameObject not found as a child of ScoreManager.");
        }
    }

    public void ResetScore()
    {
        currentCombo = 0;
        score = 0;
        totalMisses = 0;
        successfulHits = 0;
        comboScore = 0;
        maxCombo = 0;
        highestCombo = 0;
        notesHitSinceMiss = 0;
        earlyHits = 0;
        perfectHits = 0;
        lateHits = 0;
        offBeatDifferences.Clear();
        UpdateUI();
    }

    // Method to load player stats from a local JSON file
    void LoadPlayerStats()
    {
        if (File.Exists(statsFilePath))
        {
            try
            {
                string json = File.ReadAllText(statsFilePath);
                playerStats = JsonUtility.FromJson<PlayerStats>(json);
                Debug.Log("ScoreManager: Player stats loaded successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"ScoreManager: Failed to load player stats. Error: {e.Message}");
                playerStats = new PlayerStats();
            }
        }
        else
        {
            Debug.LogWarning("ScoreManager: Player stats file not found. Initializing new stats.");
            playerStats = new PlayerStats();
        }
    }

    // Method to save player stats to a local JSON file
    void SavePlayerStats()
    {
        try
        {
            string json = JsonUtility.ToJson(playerStats, true);
            File.WriteAllText(statsFilePath, json);
            Debug.Log("ScoreManager: Player stats saved successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"ScoreManager: Failed to save player stats. Error: {e.Message}");
        }
    }

    public void Hit(NoteTiming timing, float offBeatDifference)
    {
        currentCombo++;
        successfulHits++; // AI scale increment - AI
        notesHitSinceMiss++;
        if (currentCombo > maxCombo)
        {
            highestCombo = currentCombo;
        }
        score += scorePerNote * (currentCombo * comboMultiplier);

        comboScore++;
        hitSFX?.Play();
        UpdateUI();

        // Update stats
        switch (timing)
        {
            case NoteTiming.Early:
                earlyHits++;
                break;
            case NoteTiming.Perfect:
                perfectHits++;
                break;
            case NoteTiming.Late:
                lateHits++;
                break;
        }
        offBeatDifferences.Add(offBeatDifference);
    }

    // Overloaded Hit() method for compatibility with existing code
    public void Hit()
    {
        Hit(NoteTiming.Perfect, 0f);
        UpdateUI();
    }

    public void HoldNoteHit()
    {
        // Scoring logic specific to hold notes
        score += scorePerHoldTick; // Or whatever logic you prefer
        UpdateUI();
    }

    public void Miss()
    {
        totalMisses++;
        HealthManager.Instance?.TakeDamage(HealthManager.Instance.damagePerMiss); // Take damage if HealthManager exists
        comboScore = 0;
        currentCombo = 0;
        notesHitSinceMiss = 0;
        missSFX?.Play();
        UpdateUI();
    }


    // Updates the score and combo UI elements.
    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $": {score}";
        }
        if (comboText != null)
        {
            comboText.text = $": {comboScore}";
        }
    }

    public void AddHoldTickScore()
    {
        score += scorePerHoldTick;
        UpdateUI();
    }

    public void HoldNoteComplete()
    {
        currentCombo++;
        if (currentCombo > maxCombo)
        {
            maxCombo = currentCombo;
        }
        score += scorePerNote * (currentCombo * comboMultiplier);
        successfulHits++;
        comboScore++;
        hitSFX?.Play();
        UpdateUI();
    }

    // Call this method when the level is completed
    public void LevelCompleted()
    {
        // Update high score if current score is higher
        if (score > playerStats.highScore)
        {
            playerStats.highScore = (int)score; // Assuming score is an integer
        }

        // Update total notes hit and missed
        playerStats.totalNotesHit += successfulHits;
        playerStats.totalNotesMissed += totalMisses;

        // Update total early, perfect, and late hits
        playerStats.totalEarlyHits += earlyHits;
        playerStats.totalPerfectHits += perfectHits;
        playerStats.totalLateHits += lateHits;

        // Update off-beat differences
        float levelTotalOffBeatDifference = 0f;
        foreach (float diff in offBeatDifferences)
        {
            levelTotalOffBeatDifference += diff;
        }

        playerStats.totalOffBeatDifference += levelTotalOffBeatDifference;
        playerStats.totalHitsWithTiming += offBeatDifferences.Count;

        // Update level completion data
        UpdateLevelCompletion(currentLevelName, currentDifficulty);

        // Save stats to local file
        SavePlayerStats();
    }

    // Updates level completion status
    void UpdateLevelCompletion(string levelName, DifficultyLevel difficulty)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogError("ScoreManager: levelName is null or empty. Cannot update level completion data.");
            return;
        }

        LevelCompletionData levelData = playerStats.levelCompletionData.Find(l => l.levelName == levelName);

        if (levelData == null)
        {
            levelData = new LevelCompletionData { levelName = levelName };
            playerStats.levelCompletionData.Add(levelData);
            Debug.Log($"ScoreManager: Created new LevelCompletionData for '{levelName}'.");
        }

        switch (difficulty)
        {
            case DifficultyLevel.Easy:
                if (!levelData.easyCompleted)
                {
                    levelData.easyCompleted = true;
                    Debug.Log($"ScoreManager: Marked 'Easy' as completed for '{levelName}'.");
                }
                break;
            case DifficultyLevel.Normal:
                if (!levelData.normalCompleted)
                {
                    levelData.normalCompleted = true;
                    Debug.Log($"ScoreManager: Marked 'Normal' as completed for '{levelName}'.");
                }
                break;
            case DifficultyLevel.Hard:
                if (!levelData.hardCompleted)
                {
                    levelData.hardCompleted = true;
                    Debug.Log($"ScoreManager: Marked 'Hard' as completed for '{levelName}'.");
                }
                break;
            case DifficultyLevel.Expert:
                if (!levelData.expertCompleted)
                {
                    levelData.expertCompleted = true;
                    Debug.Log($"ScoreManager: Marked 'Expert' as completed for '{levelName}'.");
                }
                break;
            default:
                Debug.LogWarning($"ScoreManager: Unknown difficulty level '{difficulty}' for '{levelName}'.");
                break;
        }
    }

    public void ResetAllStats()
    {
        playerStats = new PlayerStats();
        SavePlayerStats();
        Debug.Log("ScoreManager: Player stats have been reset.");
    }

    // Methods to calculate percentages
    public float GetHitPercentage()
    {
        int totalNotes = successfulHits + totalMisses;
        if (totalNotes == 0) return 0f;
        return ((float)successfulHits / totalNotes) * 100f;
    }

    public float GetMissPercentage()
    {
        int totalNotes = successfulHits + totalMisses;
        if (totalNotes == 0) return 0f;
        return ((float)totalMisses / totalNotes) * 100f;
    }

    public float GetEarlyHitPercentage()
    {
        if (successfulHits == 0) return 0f;
        return ((float)earlyHits / successfulHits) * 100f;
    }

    public float GetPerfectHitPercentage()
    {
        if (successfulHits == 0) return 0f;
        return ((float)perfectHits / successfulHits) * 100f;
    }

    public float GetLateHitPercentage()
    {
        if (successfulHits == 0) return 0f;
        return ((float)lateHits / successfulHits) * 100f;
    }

    public float GetAverageOffBeatDifference()
    {
        if (offBeatDifferences.Count == 0) return 0f;
        float total = 0f;
        foreach (float diff in offBeatDifferences)
        {
            total += diff;
        }
        return total / offBeatDifferences.Count;
    }
}

// Supporting classes
[Serializable]
public class PlayerStats
{
    public int highScore = 0;
    public int totalNotesHit = 0;
    public int totalNotesMissed = 0;
    public int totalEarlyHits = 0;
    public int totalPerfectHits = 0;
    public int totalLateHits = 0;
    public float totalOffBeatDifference = 0f;
    public int totalHitsWithTiming = 0;
    public List<LevelCompletionData> levelCompletionData = new List<LevelCompletionData>();
}

[Serializable]
public class LevelCompletionData
{
    public string levelName;
    public bool easyCompleted = false;
    public bool normalCompleted = false;
    public bool hardCompleted = false;
    public bool expertCompleted = false;
}



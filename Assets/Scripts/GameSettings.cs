using UnityEngine;
using UnityEngine.SceneManagement;
public enum DifficultyLevel // Game difficulty level setting - AI
{
    Easy,
    Normal,
    Hard,
    Expert
}

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;
    public float speedIncrementPerHit; // Amount to increment speed per successful note hit - AI
    public float maxSpeedMultiplier; // Maximum amount of speed increment - AI
    public float maxMovementMultiplier;
    public DifficultyLevel difficulty;
    public float movementIncrementPerHit; // Spread increase per combo
    public float defaultMovementSpeed; // Units per second
    public float defaultMovementDistance; // Units
    public float maxMovementDistance = 5.0f;
    public float minMovementDistance = 0.1f;
    public float catchUpDuration; // Number of notes over which difficulty ramps back up


    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes

            // Load difficulty from PlayerPrefs, defaulting to Normal if not set
            int savedDifficulty = PlayerPrefs.GetInt("Difficulty", (int)DifficultyLevel.Normal);
            difficulty = (DifficultyLevel)savedDifficulty;
            AdjustDifficultyParameters();

            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); // There can only be one instance
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method adjusts parameters based on difficulty level - most of the difficulty balance is done from here
    // Although changing the number of beats ahead that spawns drastically changes note patterns as well
    // as it gives more or less time for notes to spread out
    public void AdjustDifficultyParameters()
    {
        switch (difficulty)
        {
            case DifficultyLevel.Easy:
                speedIncrementPerHit = 0.01f;
                maxSpeedMultiplier = 2.5f;
                maxMovementMultiplier = 1.5f;
                movementIncrementPerHit = 0.01f;
                defaultMovementSpeed = 0.1f;
                defaultMovementDistance = 1.0f;
                catchUpDuration = 20;
                break;
            case DifficultyLevel.Normal:
                speedIncrementPerHit = 0.01f;
                maxSpeedMultiplier = 3.1f;
                maxMovementMultiplier = 2.7f;
                movementIncrementPerHit = 0.1f;
                defaultMovementSpeed = 0.1f;
                defaultMovementDistance = 1.0f;
                catchUpDuration = 15;
                break;
            case DifficultyLevel.Hard:
                speedIncrementPerHit = 0.1f;
                maxSpeedMultiplier = 4.4f;
                maxMovementMultiplier = 3.1f;
                movementIncrementPerHit = 0.1f;
                defaultMovementSpeed = 0.3f;
                defaultMovementDistance = 1.2f;
                catchUpDuration = 8;
                break;
            case DifficultyLevel.Expert:
                speedIncrementPerHit = 0.2f;
                maxSpeedMultiplier = 4.9f;
                maxMovementMultiplier = 7.0f;
                movementIncrementPerHit = 0.15f;
                defaultMovementSpeed =0.5f;
                defaultMovementDistance = 1.6f;
                catchUpDuration = 6;
                break;
        }

        Debug.Log($"GameSettings updated: Difficulty set to {difficulty}");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Load difficulty from PlayerPrefs
        int savedDifficulty = PlayerPrefs.GetInt("Difficulty", (int)DifficultyLevel.Normal);
        if (difficulty != (DifficultyLevel)savedDifficulty)
        {
            difficulty = (DifficultyLevel)savedDifficulty;
            AdjustDifficultyParameters();
            Debug.Log($"GameSettings updated on scene load: Difficulty set to {difficulty}");
        }
    }

}

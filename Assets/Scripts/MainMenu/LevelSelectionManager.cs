using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// Manages the level selection UI by updating star images based on saved completion data.
public class LevelSelectionManager : MonoBehaviour
{
    [Header("Levels")]
    public List<LevelUI> levelsUI; // List to hold UI references for each level

    [Header("Star Sprites")]
    public Sprite filledStarSprite;    // Sprite for completed stars
    public Sprite unfilledStarSprite;  // Sprite for incomplete stars

    // Path to the player stats JSON file
    private string statsFilePath;

    void Start()
    {
        // Initialize the stats file path
        statsFilePath = Path.Combine(Application.persistentDataPath, "playerStats.json");
        // Load player stats
        PlayerStats playerStats = LoadPlayerStats();
        // Update the star UI based on loaded data
        UpdateStarUI(playerStats);
    }

    // Loads the player stats from the JSON file.
    PlayerStats LoadPlayerStats()
    {
        if (File.Exists(statsFilePath))
        {
            try
            {
                string json = File.ReadAllText(statsFilePath);
                PlayerStats stats = JsonUtility.FromJson<PlayerStats>(json);
                return stats;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load player stats: {e.Message}");
                return new PlayerStats();
            }
        }
        else
        {
            Debug.LogWarning("Player stats file not found. Initializing new stats.");
            return new PlayerStats();
        }
    }


    // Updates the star images in the UI based on LevelCompletionData.
    void UpdateStarUI(PlayerStats playerStats)
    {
        foreach (LevelUI levelUI in levelsUI)
        {
            Debug.Log($"Processing Level: {levelUI.levelName}");

            // Find the LevelCompletionData for this level
            LevelCompletionData levelData = playerStats.levelCompletionData.Find(l => l.levelName == levelUI.levelName);

            if (levelData == null)
            {
                Debug.LogWarning($"No LevelCompletionData found for {levelUI.levelName}. Setting all stars to unfilled.");
                // If no data found, set all stars to unfilled
                SetStarSprites(levelUI, false, false, false, false);
                continue;
            }

            Debug.Log($"Level Data for {levelUI.levelName} - Easy: {levelData.easyCompleted}, Normal: {levelData.normalCompleted}, Hard: {levelData.hardCompleted}, Expert: {levelData.expertCompleted}");

            // Set each star based on completion status
            SetStarSprites(
                levelUI,
                levelData.easyCompleted,
                levelData.normalCompleted,
                levelData.hardCompleted,
                levelData.expertCompleted
            );
        }
    }

    // Sets the star sprites for a given level UI.
    void SetStarSprites(LevelUI levelUI, bool easy, bool normal, bool hard, bool expert)
    {
        if (levelUI.easyStar != null)
        {
            levelUI.easyStar.sprite = easy ? filledStarSprite : unfilledStarSprite;
            Debug.Log($"Set {levelUI.levelName} Easy star to {(easy ? "filled" : "unfilled")}.");
        }
        else
            Debug.LogWarning($"Easy star Image is not assigned for {levelUI.levelName}.");

        if (levelUI.normalStar != null)
        {
            levelUI.normalStar.sprite = normal ? filledStarSprite : unfilledStarSprite;
            Debug.Log($"Set {levelUI.levelName} Normal star to {(normal ? "filled" : "unfilled")}.");
        }
        else
            Debug.LogWarning($"Normal star Image is not assigned for {levelUI.levelName}.");

        if (levelUI.hardStar != null)
        {
            levelUI.hardStar.sprite = hard ? filledStarSprite : unfilledStarSprite;
            Debug.Log($"Set {levelUI.levelName} Hard star to {(hard ? "filled" : "unfilled")}.");
        }
        else
            Debug.LogWarning($"Hard star Image is not assigned for {levelUI.levelName}.");

        if (levelUI.expertStar != null)
        {
            levelUI.expertStar.sprite = expert ? filledStarSprite : unfilledStarSprite;
            Debug.Log($"Set {levelUI.levelName} Expert star to {(expert ? "filled" : "unfilled")}.");
        }
        else
            Debug.LogWarning($"Expert star Image is not assigned for {levelUI.levelName}.");
    }

}


// Represents the UI components for a single level in the level selection.
[System.Serializable]
public class LevelUI
{
    public string levelName;      // e.g., "Level 1"
    public Image easyStar;
    public Image normalStar;
    public Image hardStar;
    public Image expertStar;
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

[Serializable]
public class Level
{
    [Header("Level Information")]
    public string levelName;      // e.g., "Level 1", "Level 2"
    public string sceneName;      // e.g., "akariHasArrived", "level2"

    [Header("Difficulty Buttons")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    public Button expertButton;
}


public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;    // Panel for the main menu
    public GameObject settingsPanel;    // Panel for the settings
    public GameObject levelPanel;       // Panel for level select

    [Header("First Run Image")]
    public GameObject firstRunImage;
    public Button firstRunCloseButton;

    [Header("Tutorial Panels")]
    public GameObject tutorial1Panel;
    public GameObject tutorial2Panel;
    public GameObject tutorial3Panel;
    public GameObject tutorial4Panel;
    public GameObject tutorial5Panel;

    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public Button mainTutorialButton;   // New Tutorial button

    [Header("Settings Components")]
    public Button settingsBackButton;
    public Slider settingsHitVolumeSlider;

    [Header("Level Selection Buttons")]
    public Button levelBackButton;

    [Header("Tutorial Buttons")]
    // Tutorial1 Buttons
    public Button tutorial1BackButton;
    public Button tutorial1NextButton;

    // Tutorial2 Buttons
    public Button tutorial2BackButton;
    public Button tutorial2NextButton;

    // Tutorial3 Buttons
    public Button tutorial3BackButton;
    public Button tutorial3NextButton;

    // Tutorial4 Buttons
    public Button tutorial4BackButton;
    public Button tutorial4NextButton;

    // Tutorial5 Buttons
    public Button tutorial5BackButton;
    public Button tutorial5BackToMainButton;

    [Header("Levels")]
    public List<Level> levels;   // List to hold multiple levels

    private float inputLag = 0;

    private void Start()
    {
        // Start in main menu
        ShowMainMenu();

        // Initialize main menu buttons
        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        mainTutorialButton.onClick.AddListener(OnMainTutorialClicked); // Assign Tutorial button

        // Initialize settings buttons
        settingsBackButton.onClick.AddListener(OnBackToMainMenuFromSettings);

        // Initialize level selection buttons
        levelBackButton.onClick.AddListener(OnBackToMainMenuFromLevelSelect);

        // Initialize settings sliders
        InitializeSettings();

        // Initialize tutorial buttons
        InitializeTutorialButtons();

        // Initialize levels and assign listeners
        InitializeLevels();

                // Check if it's the first run
        if (PlayerPrefs.GetInt("FirstRun", 0) == 0)
        {
            // First run logic
            ShowFirstRunImage();

            // Set the flag to indicate that the game has been run before
            PlayerPrefs.SetInt("FirstRun", 1);
            PlayerPrefs.Save();
        }
        else
        {
            // Not the first run, proceed as normal
            ShowMainMenu();
        }
    }

    /// <summary>
    /// Initializes settings sliders and loads saved settings.
    /// </summary>
    void InitializeSettings()
    {
        if (settingsHitVolumeSlider != null)
        {
            // Retrieve the saved hit sound volume or default to 1 if not set
            float hitSoundVolume = PlayerPrefs.GetFloat("HitSoundVolume", 1f); // Default volume is 1
            settingsHitVolumeSlider.value = hitSoundVolume;

            // Add listener for the settings hit sound volume slider
            settingsHitVolumeSlider.onValueChanged.AddListener(SetHitSoundVolume);
        }
        else
        {
            Debug.LogWarning("MainMenuManager: settingsHitVolumeSlider is not assigned.");
        }
    }

    /// <summary>
    /// Handles changes to the hit sound volume slider.
    /// </summary>
    /// <param name="value">The new volume value.</param>
    public void SetHitSoundVolume(float value)
    {
        // Save the volume preference
        PlayerPrefs.SetFloat("HitSoundVolume", value);
        PlayerPrefs.Save();

        // If the ScoreManager instance exists, update the hitSFX volume
        if (ScoreManager.Instance != null && ScoreManager.Instance.hitSFX != null)
        {
            ScoreManager.Instance.hitSFX.volume = value;
        }
    }

    /// <summary>
    /// Displays the first run image.
    /// </summary>
    void ShowFirstRunImage()
    {
        // Activate the FirstRunImage GameObject
        if (firstRunImage != null)
        {
            firstRunImage.SetActive(true);

            // Ensure the main menu remains active
            mainMenuPanel.SetActive(true);

            // Assign the OnClick listener to the close button
            if (firstRunCloseButton != null)
            {
                firstRunCloseButton.onClick.RemoveAllListeners(); // Clear any existing listeners
                firstRunCloseButton.onClick.AddListener(OnCloseFirstRunImage);
            }
            else
            {
                Debug.LogWarning("MainMenuManager: FirstRunCloseButton is not assigned.");
            }
        }
        else
        {
            Debug.LogWarning("MainMenuManager: FirstRunImage is not assigned.");
        }
    }

    /// <summary>
    /// Called when the player closes the first run image.
    /// </summary>
    public void OnCloseFirstRunImage()
    {
        if (firstRunImage != null)
        {
            firstRunImage.SetActive(false);
        }
        else
        {
            Debug.LogWarning("MainMenuManager: FirstRunImage is not assigned.");
        }

        // The main menu remains active
    }


    // Initialize all levels and their difficulty buttons
    void InitializeLevels()
    {
        foreach (Level level in levels)
        {
            // Assign listeners to difficulty buttons if they are assigned
            if (level.easyButton != null)
                level.easyButton.onClick.AddListener(() => OnDifficultyClicked(level, DifficultyLevel.Easy));

            if (level.normalButton != null)
                level.normalButton.onClick.AddListener(() => OnDifficultyClicked(level, DifficultyLevel.Normal));

            if (level.hardButton != null)
                level.hardButton.onClick.AddListener(() => OnDifficultyClicked(level, DifficultyLevel.Hard));

            if (level.expertButton != null)
                level.expertButton.onClick.AddListener(() => OnDifficultyClicked(level, DifficultyLevel.Expert));
        }
    }

    // Initialize all tutorial buttons and assign listeners
    void InitializeTutorialButtons()
    {
        // Tutorial1 Buttons
        if (tutorial1BackButton != null)
            tutorial1BackButton.onClick.AddListener(() => OnTutorialBackClicked(tutorial1Panel, mainMenuPanel));

        if (tutorial1NextButton != null)
            tutorial1NextButton.onClick.AddListener(() => OnTutorialNextClicked(tutorial1Panel, tutorial2Panel));

        // Tutorial2 Buttons
        if (tutorial2BackButton != null)
            tutorial2BackButton.onClick.AddListener(() => OnTutorialBackClicked(tutorial2Panel, tutorial1Panel));

        if (tutorial2NextButton != null)
            tutorial2NextButton.onClick.AddListener(() => OnTutorialNextClicked(tutorial2Panel, tutorial3Panel));

        // Tutorial3 Buttons
        if (tutorial3BackButton != null)
            tutorial3BackButton.onClick.AddListener(() => OnTutorialBackClicked(tutorial3Panel, tutorial2Panel));

        if (tutorial3NextButton != null)
            tutorial3NextButton.onClick.AddListener(() => OnTutorialNextClicked(tutorial3Panel, tutorial4Panel));

        // Tutorial4 Buttons
        if (tutorial4BackButton != null)
            tutorial4BackButton.onClick.AddListener(() => OnTutorialBackClicked(tutorial4Panel, tutorial3Panel));

        if (tutorial4NextButton != null)
            tutorial4NextButton.onClick.AddListener(() => OnTutorialNextClicked(tutorial4Panel, tutorial5Panel));

        // Tutorial5 Buttons
        if (tutorial5BackButton != null)
            tutorial5BackButton.onClick.AddListener(() => OnTutorialBackClicked(tutorial5Panel, tutorial4Panel));

        if (tutorial5BackToMainButton != null)
            tutorial5BackToMainButton.onClick.AddListener(() => OnTutorialBackToMainClicked(tutorial5Panel));
    }

    // Method called when a difficulty button is clicked
    void OnDifficultyClicked(Level level, DifficultyLevel difficulty)
    {
        SetDifficulty(difficulty);
        LoadGameScene(level.sceneName);
    }

    // Method to set difficulty
    void SetDifficulty(DifficultyLevel difficulty)
    {
        PlayerPrefs.SetInt("Difficulty", (int)difficulty);
        PlayerPrefs.Save();

        Debug.Log($"Difficulty set to: {difficulty}");
    }

    // Method to load the game scene
    void LoadGameScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    void OnStartClicked()
    {
        levelPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    void OnSettingsClicked()
    {
        // Show settings menu
        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);

        // Update settings slider to reflect the current value
        if (settingsHitVolumeSlider != null)
        {
            float hitSoundVolume = PlayerPrefs.GetFloat("HitSoundVolume", 1f);
            settingsHitVolumeSlider.value = hitSoundVolume;
        }
    }
    
    void OnExitClicked()
    {
        // Exit the game
        Application.Quit();
    }

    public void OnBackToMainMenuFromSettings()
    {
        // Go back to main menu from settings
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    void OnBackToMainMenuFromLevelSelect()
    {
        // Go back to main menu from level select
        levelPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Method to handle mainTutorialButton click
    void OnMainTutorialClicked()
    {
        mainMenuPanel.SetActive(false);
        tutorial1Panel.SetActive(true);
    }

    // Method to handle Back button clicks in tutorials
    void OnTutorialBackClicked(GameObject currentTutorial, GameObject previousPanel)
    {
        if (currentTutorial != null && previousPanel != null)
        {
            currentTutorial.SetActive(false);
            previousPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("MainMenuManager: Tutorial panels or previous panels are not assigned correctly.");
        }
    }

    // Method to handle Next button clicks in tutorials
    void OnTutorialNextClicked(GameObject currentTutorial, GameObject nextTutorial)
    {
        if (currentTutorial != null && nextTutorial != null)
        {
            currentTutorial.SetActive(false);
            nextTutorial.SetActive(true);
        }
        else
        {
            Debug.LogError("MainMenuManager: Tutorial panels or next panels are not assigned correctly.");
        }
    }

    // Method to handle BackToMain button click in Tutorial5
    void OnTutorialBackToMainClicked(GameObject currentTutorial)
    {
        if (currentTutorial != null)
        {
            currentTutorial.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("MainMenuManager: Current Tutorial panel is not assigned correctly.");
        }
    }

    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        levelPanel.SetActive(false);

        // Ensure all tutorial panels are hidden at start
        HideAllTutorialPanels();
        
        // Show and unlock the cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Utility method to hide all tutorial panels
    void HideAllTutorialPanels()
    {
        if (tutorial1Panel != null)
            tutorial1Panel.SetActive(false);
        if (tutorial2Panel != null)
            tutorial2Panel.SetActive(false);
        if (tutorial3Panel != null)
            tutorial3Panel.SetActive(false);
        if (tutorial4Panel != null)
            tutorial4Panel.SetActive(false);
        if (tutorial5Panel != null)
            tutorial5Panel.SetActive(false);
    }


    [ContextMenu("Reset First Run")]
    public void ResetFirstRun()
    {
        PlayerPrefs.DeleteKey("FirstRun");
        PlayerPrefs.Save();
        Debug.Log("MainMenuManager: FirstRun flag has been reset.");
    }

}


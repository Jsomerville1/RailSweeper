// PauseMenu.cs
using UnityEngine;
using UnityEngine.UI; // Needed for UI components
using UnityEngine.SceneManagement; // Needed for scene management

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI; // Assign your pause panel in the Inspector
    [SerializeField] private bool isPaused = false;
    /// <summary>
    /// Tracks paused state for reference elsewhere in codebase
    /// </summary>
    public static bool GamePaused = false;

    public Button resume, quit, restart, returnToMain;
    public Toggle hitSound;
    private GameSettings gameSettings;
    public Slider hitSoundVolumeSlider;
    public Slider gameAudioLevelSlider; // Slider for overall game volume

    // **Flag to track UI state**
    private bool isUIActive = false;

    void Start()
    {
        GamePaused = false; // Ensure the game starts unpaused
        resume.onClick.AddListener(Resume);
        quit.onClick.AddListener(QuitGame);
        restart.onClick.AddListener(Restart); // Listener for restart button
        returnToMain.onClick.AddListener(ReturnToMainMenu); // Listener for returnToMain button
        hitSound.onValueChanged.AddListener(toggleHitSound);

        // Listener for the game audio level slider
        gameAudioLevelSlider.onValueChanged.AddListener(SetGameAudioLevel);

        // Find the GameSettings object in the scene
        gameSettings = FindObjectOfType<GameSettings>();

        Time.timeScale = 1f; // Ensure the game starts unpaused
        pauseMenuUI.SetActive(false); // Hide the pause menu initially

        // Hide and lock the cursor at the start of the game
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Initialize Hit Sound Toggle
        bool isHitSoundEnabled = PlayerPrefs.GetInt("HitSoundEnabled", 1) == 1; // Default to enabled
        hitSound.isOn = isHitSoundEnabled;

        // Initialize Hit Sound Volume Slider
        float hitSoundVolume = PlayerPrefs.GetFloat("HitSoundVolume", 1f); // Default volume is 1
        hitSoundVolumeSlider.value = hitSoundVolume; // Set slider value
        hitSoundVolumeSlider.onValueChanged.AddListener(SetHitSoundVolume);

        // Set the hit sound volume to the saved value
        SetHitSoundVolume(hitSoundVolume);

        // Initialize Game Audio Level Slider
        float gameAudioLevel = PlayerPrefs.GetFloat("GameAudioLevel", 1f); // Default volume is 1
        gameAudioLevelSlider.value = gameAudioLevel; // Set slider value
        gameAudioLevelSlider.onValueChanged.AddListener(SetGameAudioLevel);

        // Set the game audio level to the saved value
        SetGameAudioLevel(gameAudioLevel);

        // Subscribe to UIManager Events
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIOpened += OnUIOpened;
            UIManager.Instance.OnUIClosed += OnUIClosed;
        }
        else
        {
            Debug.LogError("UIManager.Instance is null. Please ensure UIManager is added to the scene.");
        }
    }

    void Update()
    {
        // Check for Esc key press to toggle the pause state
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // Pauses the game
    public void Pause()
    {
        float hitSoundVolume = PlayerPrefs.GetFloat("HitSoundVolume", 1f); // Default volume is 1
        hitSoundVolumeSlider.value = hitSoundVolume; // Set slider value

        bool isHitSoundEnabled = PlayerPrefs.GetInt("HitSoundEnabled", 1) == 1; // Default to enabled
        hitSound.isOn = isHitSoundEnabled;

        pauseMenuUI.SetActive(true); // Show the pause menu UI
        isPaused = true;
        GamePaused = true;
        SongManager.Instance.Pause();
        // Notify UIManager
        UIManager.Instance.OpenUIPanel();

        // Show and unlock the cursor when paused
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Resumes the game
    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu UI
        isPaused = false;
        GamePaused = false;
        SongManager.Instance.Resume();
        // Notify UIManager
        UIManager.Instance.CloseUIPanel();

        // Hide and lock the cursor when resuming
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Restart the level (current scene reload)
    public void Restart()
    {
        // Hide pause menu
        pauseMenuUI.SetActive(false);
        // Notify UIManager
        UIManager.Instance.CloseUIPanel();
        isPaused = false;
        Time.timeScale = 1f; // Ensure the game is running when the scene reloads
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
    }

    // Return to the main menu
    public void ReturnToMainMenu()
    {
        // Hide pause menu
        pauseMenuUI.SetActive(false);
        // Notify UIManager
        UIManager.Instance.CloseUIPanel();

        Time.timeScale = 1f; // Ensure the game is running
        isPaused = false;
        SceneManager.LoadScene("MainMenu"); // Load the MainMenu scene
    }

    // Quit the game
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit(); // Quit the game
    }

    public void toggleHitSound(bool isOn)
    {
        // Enable or disable the hitSFX AudioSource
        if (ScoreManager.Instance != null && ScoreManager.Instance.hitSFX != null)
        {
            ScoreManager.Instance.hitSFX.enabled = isOn;

            // Save the preference using PlayerPrefs
            PlayerPrefs.SetInt("HitSoundEnabled", isOn ? 1 : 0);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError("ScoreManager.Instance or ScoreManager.Instance.hitSFX is null.");
        }
    }

    // Method to handle Hit Sound Volume changes
    public void SetHitSoundVolume(float value)
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.hitSFX != null)
        {
            ScoreManager.Instance.hitSFX.volume = value;

            // Save the volume preference
            PlayerPrefs.SetFloat("HitSoundVolume", value);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError("ScoreManager.Instance or ScoreManager.Instance.hitSFX is null.");
        }
    }

    // Method to handle Game Audio Level changes
    public void SetGameAudioLevel(float value)
    {
        // Adjust the volume of the AudioListener
        AudioListener.volume = value;

        // Save the volume preference
        PlayerPrefs.SetFloat("GameAudioLevel", value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Event handler for when a UI panel is opened.
    /// Disables certain functionalities if necessary.
    /// </summary>
    private void OnUIOpened()
    {
        isUIActive = true;
    }

    /// <summary>
    /// Event handler for when a UI panel is closed.
    /// Enables certain functionalities if necessary.
    /// </summary>
    private void OnUIClosed()
    {
        isUIActive = false;
    }

    void OnDestroy()
    {
        // Unsubscribe from UIManager Events to Prevent Memory Leaks
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIOpened -= OnUIOpened;
            UIManager.Instance.OnUIClosed -= OnUIClosed;
        }
    }
}

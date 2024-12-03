using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;

public class InputLagManager : MonoBehaviour
{
    [Header("Audio Components")]
    public AudioSource metronome; // Assign the metronome AudioSource in the Inspector
    public string metronomeMidiFilePath; // Path to the metronome MIDI file (e.g., "Metronome.mid")
    
    [Header("UI Components")]
    public Button startBeatButton;
    public Button offsetButton;
    public TextMeshProUGUI countdownText;
    public TMP_InputField inputLagInputField; // Editable input field for input lag
    public TextMeshProUGUI averageDelayText; // Text to display "Average Delay: x ms"
    public Slider settingsVolumeSlider; // Slider to adjust game volume
    public Button settingsApplyButton;
    
    [Header("Calibration Settings")]
    public float countdownDuration = 3f; // Countdown duration in seconds
    public int numberOfBeatsToMeasure = 4; // Number of beats to measure for average delay
    
    [Header("MainMenuManager Reference")]
    public MainMenuManager mainMenuManager; // Reference to MainMenuManager to handle panel navigation
    
    private List<double> beatTimes = new List<double>(); // Expected beat times relative to song start
    private List<float> playerClickTimes = new List<float>(); // Player's click times relative to song start
    private bool isCalibrating = false;
    private int currentBeatIndex = 0;
    private double calibrationStartDspTime;
    public static InputLagManager Instance;
    [Header("Menu Music")]
    public AudioSource menuMusic; // Assign the MenuMusic AudioSource in the Inspector

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Other initialization
        }
        else
        {
            Destroy(gameObject);
        }
    }



    /// Retrieves the input lag from PlayerPrefs and converts it to seconds.
    public float GetInputLagInSeconds()
    {
        float inputLagMs = PlayerPrefs.GetFloat("InputLag", 0f);
        return inputLagMs / 1000f; // Convert milliseconds to seconds
    }


    void Start()
    {
        // Initialize UI components
        startBeatButton.onClick.AddListener(OnStartBeatButtonClicked);
        offsetButton.onClick.AddListener(OnOffsetButtonClicked);
        settingsApplyButton.onClick.AddListener(OnSettingsApplyClicked);
        
        // Initially, offsetButton is visible but non-functional until calibration starts
        offsetButton.interactable = false;
        countdownText.gameObject.SetActive(false);
        
        // Load metronome MIDI file and extract beat times
        LoadMetronomeBeats();
        
        // Initialize input lag input field with saved value or default
        float savedLag = PlayerPrefs.GetFloat("InputLag", 0);
        inputLagInputField.text = savedLag.ToString("F0");
        
        // Initialize volume slider with saved value or default
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        settingsVolumeSlider.value = savedVolume;
        UpdateGameVolume(savedVolume);
        
        // Assign listener to volume slider
        settingsVolumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
    }
    

    // Loads the metronome MIDI file and extracts the beat times.
    // Assumes that the MIDI file's notes are already in chronological order.
    void LoadMetronomeBeats()
    {
        try
        {
            // Read the MIDI file
            var midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + metronomeMidiFilePath);
            var tempoMap = midiFile.GetTempoMap();
            var notes = midiFile.GetNotes();
            
            // Extract beat times based on note start times
            // Assuming each note corresponds to a beat and are already in order
            foreach (var note in notes)
            {
                double timeInSeconds = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap).TotalSeconds;
                beatTimes.Add(timeInSeconds);
            }
            
            Debug.Log($"InputLagManager: Loaded {beatTimes.Count} beats from MIDI.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"InputLagManager: Failed to load metronome MIDI file. {ex.Message}");
        }
    }
    

    // Handler for the Start Beat Button click.
    // Begins the calibration process with a countdown.
    void OnStartBeatButtonClicked()
    {
        if (!isCalibrating)
        {
            StartCoroutine(CalibrationCountdown());
        }
    }
    

    // Coroutine for the countdown before calibration starts.
    IEnumerator CalibrationCountdown()
    {
        isCalibrating = true;
        startBeatButton.interactable = false;
        offsetButton.interactable = false; // Disable during countdown
        countdownText.gameObject.SetActive(true);
        
        float remainingTime = countdownDuration;
        while (remainingTime > 0)
        {
            countdownText.text = Mathf.CeilToInt(remainingTime).ToString();
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }
        
        countdownText.text = "Go!";
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
        
        // Pause the menu music
        if (menuMusic != null && menuMusic.isPlaying)
        {
            menuMusic.Pause();
            Debug.Log("InputLagManager: MenuMusic paused for calibration.");
        }
        else
        {
            Debug.LogWarning("InputLagManager: MenuMusic AudioSource is null or not playing.");
        }
        


        // Start the metronome and calibration
        metronome.Play();
        calibrationStartDspTime = AudioSettings.dspTime;
        currentBeatIndex = 0;
        playerClickTimes.Clear();
        offsetButton.interactable = true; // Enable after countdown
        
        isCalibrating = false;
    }
    
    // Handler for the Offset Button click.
    // Records the player's click time and calculates the delay.
    void OnOffsetButtonClicked()
    {
        if (currentBeatIndex < numberOfBeatsToMeasure && currentBeatIndex < beatTimes.Count)
        {
            // Calculate the expected beat time relative to calibration start
            double expectedBeatTime = beatTimes[currentBeatIndex];
            
            // Current time relative to calibration start
            double currentDspTime = AudioSettings.dspTime;
            float elapsedTime = (float)(currentDspTime - calibrationStartDspTime);
            
            // Calculate delay: player click time - expected beat time
            float delay = elapsedTime - (float)expectedBeatTime;
            playerClickTimes.Add(delay);
            
            Debug.Log($"InputLagManager: Beat {currentBeatIndex + 1}, Expected: {expectedBeatTime:F3}s, Clicked at: {elapsedTime:F3}s, Delay: {delay:F3}s");
            
            currentBeatIndex++;
            
            if (currentBeatIndex >= numberOfBeatsToMeasure || currentBeatIndex >= beatTimes.Count)
            {
                // Calibration complete
                metronome.Stop();
                offsetButton.interactable = false;
                startBeatButton.interactable = true;
                CalculateAndDisplayAverageDelay();
            }
            else
            {
                // Decided not to implement visual cue...
            }
        }
    }
    

    // Calculates the average input lag and updates the UI.
    // Also updates the inputLagInputField and averageDelayText to reflect the measured delay.
    void CalculateAndDisplayAverageDelay()
    {
        if (playerClickTimes.Count == 0)
        {
            inputLagInputField.text = "0";
            averageDelayText.text = "Average Delay: 0 ms";
            return;
        }
        
        float sum = 0f;
        foreach (float delay in playerClickTimes)
        {
            sum += delay;
        }
        float averageDelay = sum / playerClickTimes.Count;
        float averageDelayMs = averageDelay * 1000f; // Convert to milliseconds
        
        inputLagInputField.text = averageDelayMs.ToString("F0");
        averageDelayText.text = $"Average Delay: {averageDelayMs:F0} ms";
        
        Debug.Log($"InputLagManager: Average Input Lag: {averageDelayMs:F0} ms");

                
        // Resume the menu music
        ResumeMenuMusic();
    }
    
    // Resumes the MenuMusic AudioSource
    void ResumeMenuMusic()
    {
        if (menuMusic != null)
        {
            menuMusic.UnPause();
            Debug.Log("InputLagManager: MenuMusic resumed after calibration.");
        }
        else
        {
            Debug.LogWarning("InputLagManager: MenuMusic AudioSource is not assigned.");
        }
    }



    // Handler for the Settings Apply Button click.
    // Saves the input lag and volume values from the input field and slider,
    // then returns to the main menu.
    void OnSettingsApplyClicked()
    {
        // Parse the inputLagInputField text to a float
        float selectedDelay;
        if (float.TryParse(inputLagInputField.text, out selectedDelay))
        {
            // Clamp the value to a reasonable range (e.g., 0 to 500 ms)
            selectedDelay = Mathf.Clamp(selectedDelay, 0f, 500f);
            inputLagInputField.text = selectedDelay.ToString("F0"); // Update text to clamped value
        }
        else
        {
            // If parsing fails, reset to the previous saved value
            selectedDelay = PlayerPrefs.GetFloat("InputLag", 0);
            inputLagInputField.text = selectedDelay.ToString("F0");
            Debug.LogWarning("InputLagManager: Invalid input lag value entered. Reverting to previous value.");
        }
        
        // Get the volume from the slider
        float selectedVolume = settingsVolumeSlider.value;
        
        // Save the settings
        PlayerPrefs.SetFloat("InputLag", selectedDelay);
        PlayerPrefs.SetFloat("GameVolume", selectedVolume);
        PlayerPrefs.Save();
        
        Debug.Log($"InputLagManager: Input Lag set to {selectedDelay} ms");
        Debug.Log($"InputLagManager: Game Volume set to {selectedVolume}");
        
        // Return to main menu
        if (mainMenuManager != null)
        {
            mainMenuManager.OnBackToMainMenuFromSettings();
        }
        else
        {
            Debug.LogError("InputLagManager: MainMenuManager reference is not set.");
        }
    }
    

    // Handler for the Volume Slider value change.
    // Updates the game volume in real-time.
    void OnVolumeSliderChanged(float value)
    {
        UpdateGameVolume(value);
    }
    

    // Updates the overall game volume.
    void UpdateGameVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log($"InputLagManager: Game volume set to {volume}");
    }
}

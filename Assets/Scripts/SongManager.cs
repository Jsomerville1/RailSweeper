using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;

    [Header("Audio Settings")]
    public AudioSource musicSource;
    public string fileLocation; 
    [Header("Timing Settings")]
    public float firstBeatOffset;
    public float noteSpawnAdjuster;
    public float beatsShownInAdvance; // Number of beats to show in advance
    public float distanceBetweenNotes; // Unity units per beat
    public float noteSpawnDelay = 5f; // Delay before notes and music start
    [Header("Lanes")]
    public Lane[] lanes; // Assign all your lanes in the Inspector

    [HideInInspector]
    public MidiFile midiFile;

    public float bpm;
    public float currentBeat;
    public float secPerBeat;

    [SerializeField] private double songStartDspTime;
    [SerializeField] private double currentDspTime;
    private bool isPlaying = false; // Track when the music starts playing
    [SerializeField] private int frameCap = 60;
    [SerializeField] private double pauseStartDspTime; // when the pause started

    private bool isPaused = false; // pause flag
    private bool levelEnded = false;

    void Awake()
    {

        Debug.Log("SongManager Awake called.");
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.Log("Another SongManager instance exists. Destroying this one.");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;


    }

    void Update()
    {
        if (levelEnded)
            return;

        // Check if music has finished playing
        if (isPlaying && !musicSource.isPlaying && !isPaused)
        {
            // Music has finished
            LevelCompleted();
        }
    }

    private void LevelCompleted()
    {
        levelEnded = true;
        isPlaying = false;

        // Update player stats, save data, etc.
        ScoreManager.Instance.LevelCompleted();

        // Show Level Complete UI
        LevelEndUI.Instance.ShowLevelComplete();
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
        isPlaying = false;
    }



    void Start()
    {
        Debug.Log("SongManager Start called.");

        Application.targetFrameRate = frameCap; // limit FPS to 60

        // Validate BPM
        if (bpm <= 0f)
        {
            Debug.LogError("SongManager: BPM must be greater than zero.");
            return;
        }
        // Calculate secPerBeat
        secPerBeat = 60f / bpm;
        Debug.Log($"SongManager: secPerBeat calculated as: {secPerBeat}");

        // Initialize lanes with distance between notes and beats shown in advance
        foreach (var lane in lanes)
        {
            lane.InitializeLane(distanceBetweenNotes, beatsShownInAdvance);
        }
        midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + fileLocation);

        var notes = midiFile.GetNotes();
        var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];
        notes.CopyTo(array, 0);
        foreach (var lane in lanes)
        {
            lane.SetTimeStamps(array);
        }
        // Start a delayed coroutine for the music and note spawning
        StartCoroutine(PreloadAndStartMusic());

    }

    // Coroutine to preload music and start it after a delay.
    IEnumerator PreloadAndStartMusic()
    {
        // Check if the AudioSource is assigned
        if (musicSource != null)
        {
            // Preload the audio data
            musicSource.clip.LoadAudioData(); // Make sure the audio clip is loaded into memory

            // Wait for the clip to finish loading
            while (musicSource.clip.loadState == AudioDataLoadState.Loading)
            {
                yield return null; // Continue waiting while the clip is still loading
            }

            // Check if the loading succeeded
            if (musicSource.clip.loadState == AudioDataLoadState.Loaded)
            {
                Debug.Log("Audio clip fully preloaded.");

                // Now that everything is loaded, start the train movement
                TrainController.Instance.SetSpeed(distanceBetweenNotes, bpm);
                Debug.Log("Train speed set. Train starts moving.");

                // Wait for the noteSpawnDelay before starting the music
                yield return new WaitForSeconds(noteSpawnDelay);

                songStartDspTime = AudioSettings.dspTime - firstBeatOffset;
                musicSource.Play();
                isPlaying = true;
                Debug.Log($"Music started at DSP Time: {songStartDspTime}");

            }
            else
            {
                Debug.LogError("Failed to load audio clip. Please check the audio file.");
            }
        }
        else
        {
            Debug.LogError("MusicSource is not assigned in SongManager.");
        }
    }



    // Returns the current song position in beats based on dspTime.
    public float GetSongPositionInBeats()
    {
        if (!isPlaying || musicSource == null || PauseMenu.GamePaused)
        {
            return 0f;
        }
        currentDspTime = AudioSettings.dspTime;
        double elapsedTime = currentDspTime - songStartDspTime;
        currentBeat = (float)(elapsedTime / secPerBeat);
        return currentBeat;
    }

    public bool CanSpawnNotes()
    {
        return isPlaying; // Only allow note spawning after the music starts
    }

    public void Pause()
    {
        if (!isPaused)
        {
            isPaused = true;
            musicSource.Pause();
            Time.timeScale = 0f;
            pauseStartDspTime = AudioSettings.dspTime; // record the time that the pause condition started
        }
    }

    public void Resume()
    {
        if (isPaused)
        {
            isPaused = false;
            musicSource.UnPause();
            Time.timeScale = 1f;
            double pauseEndDspTime = AudioSettings.dspTime;
            double pauseDuration = pauseEndDspTime - pauseStartDspTime;
            songStartDspTime += pauseDuration; // adjust song time by the duration of the pause condition
        }
    }

    public bool IsPaused()
    {
        return isPaused;
    }

}

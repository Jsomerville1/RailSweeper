using UnityEngine;

public class LevelSelectAudioManager : MonoBehaviour
{
    public static LevelSelectAudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource menuMusic;          // Assign the MenuMusic AudioSource in the Inspector
    public AudioSource sampleAudioSource;  // AudioSource for playing sample clips

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure the sampleAudioSource is assigned
        if (sampleAudioSource == null)
        {
            sampleAudioSource = gameObject.AddComponent<AudioSource>();
            sampleAudioSource.playOnAwake = false;
            sampleAudioSource.loop = true; // Loop the sample clip if desired
        }
    }

    /// <summary>
    /// Plays the given sample clip and pauses the menu music.
    /// </summary>
    /// <param name="clip">The sample AudioClip to play.</param>
    public void PlaySampleClip(AudioClip clip)
    {
        // Pause the menu music
        if (menuMusic != null && menuMusic.isPlaying)
        {
            menuMusic.Pause();
        }

        // Stop any currently playing sample clip
        if (sampleAudioSource.isPlaying)
        {
            sampleAudioSource.Stop();
        }

        // Play the new sample clip
        if (clip != null)
        {
            sampleAudioSource.clip = clip;
            sampleAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("LevelSelectAudioManager: No sample clip assigned to play.");
        }
    }

    /// <summary>
    /// Stops the sample clip and resumes the menu music.
    /// </summary>
    public void StopSampleClip()
    {
        // Stop the sample clip
        if (sampleAudioSource.isPlaying)
        {
            sampleAudioSource.Stop();
        }

        // Resume the menu music
        if (menuMusic != null)
        {
            menuMusic.UnPause();
        }
    }
}

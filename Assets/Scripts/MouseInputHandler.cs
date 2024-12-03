// MouseInputHandler.cs
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NoteComparer : IComparer<Note>
{
    public int Compare(Note x, Note y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        int beatComparison = x.beatOfThisNote.CompareTo(y.beatOfThisNote);
        if (beatComparison != 0)
        {
            return beatComparison;
        }

        // If beats are equal, use instance IDs to differentiate
        return x.GetInstanceID().CompareTo(y.GetInstanceID());
    }
}

public class MouseInputHandler : MonoBehaviour
{
    public static MouseInputHandler Instance; // Singleton instance

    public Camera mainCamera;
    public LayerMask noteLayerMask;
    //[SerializeField] private List<Note> activeNotes = new List<Note>(); // List of active notes

    [Header("Hit Feedback Texts")]
    [SerializeField] public TextMeshProUGUI earlyText;
    [SerializeField] public TextMeshProUGUI perfectText;
    [SerializeField] public TextMeshProUGUI lateText;
    [SerializeField] public TextMeshProUGUI missText;
    [SerializeField] private SortedSet<Note> activeNotes; // Sorted Set will keep the lowest beat note at the top of the list
    [SerializeField] Note closestNote;
    private float inputLagInSeconds;

    // **Flag to track UI state**
    private bool isUIActive = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            activeNotes = new SortedSet<Note>(new NoteComparer());

        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Ensure UIManager is present
        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager.Instance is null. Please ensure UIManager is added to the scene.");
        }

        if (noteLayerMask == 0)
        {
            Debug.LogWarning("Note Layer Mask is not set in MouseInputHandler.");
        }

        // **Subscribe to UIManager Events**
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIOpened += OnUIOpened;
            UIManager.Instance.OnUIClosed += OnUIClosed;
        }

        // **Retrieve input lag directly from PlayerPrefs**
        float inputLagMs = PlayerPrefs.GetFloat("InputLag", 0f);
        inputLagInSeconds = inputLagMs / 1000f; // Convert milliseconds to seconds
        Debug.Log($"MouseInputHandler: Input lag set to {inputLagInSeconds} seconds.");

    }

    void Update()
    {
        // **Do not process input if UI is active**
        if (isUIActive)
        {
            return;
        }

        // Check for mouse button click (left or right)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            HandleMouseClick();
        }

        if (activeNotes.Count == 0)
        {
            // No notes to process
            return;
        }

        //closestNote = activeNotes.Min;
    }

    void HandleMouseClick()
    {
        closestNote = activeNotes.Min;

        if (closestNote != null && closestNote.canBePressed)
        {
            // Cast ray from the center of the screen
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            // Visualize the ray in the Scene view
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1f);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, noteLayerMask))
            {
                Note hitNote = hit.collider.GetComponent<Note>();

                if (hitNote == closestNote)
                {
                    // Determine hit accuracy based on current zone
                    HitAccuracy hitAccuracy;

                    switch (hitNote.currentZone)
                    {
                        case NoteZone.Early:
                            hitAccuracy = HitAccuracy.Early;
                            break;
                        case NoteZone.Perfect:
                            hitAccuracy = HitAccuracy.Perfect;
                            break;
                        case NoteZone.Late:
                            hitAccuracy = HitAccuracy.Late;
                            break;
                        default:
                            hitAccuracy = HitAccuracy.Miss;
                            break;
                    }
                    
                    // Calculate offBeatDifference
                    float inputLagInBeats = inputLagInSeconds / SongManager.Instance.secPerBeat;
                    float adjustedCurrentBeat = SongManager.Instance.currentBeat - inputLagInBeats;
                    float offBeatDifference = adjustedCurrentBeat - hitNote.beatOfThisNote;

                    if (hitAccuracy != HitAccuracy.Miss)
                    {
                        // Successful hit
                        hitNote.OnHit(GetNoteTiming(hitAccuracy), offBeatDifference);

                        // Display appropriate feedback
                        switch (hitAccuracy)
                        {
                            case HitAccuracy.Early:
                                DisplayAndFadeText(earlyText);
                                //Debug.Log($"Note hit was in: {HitAccuracy.Early}");
                                break;
                            case HitAccuracy.Perfect:
                                DisplayAndFadeText(perfectText);
                                //Debug.Log($"Note hit was in: {HitAccuracy.Perfect}");
                                break;
                            case HitAccuracy.Late:
                                DisplayAndFadeText(lateText);
                                //Debug.Log($"Note hit was in: {HitAccuracy.Late}");
                                break;
                        }

                        //Debug.Log($"Hit note at beat {hitNote.beatOfThisNote}, accuracy: {hitAccuracy}, SONGBEAT: {SongManager.Instance.currentBeat}");
                    }
                    else
                    {
                        // Missed the note due to being outside any zone
                        Debug.Log($"Missed note at beat {hitNote.beatOfThisNote} due to being outside any zone.");
                        DisplayAndFadeText(missText);
                    }
                }
                else
                {
                    // The clicked note is not the closest one
                    //Debug.Log("Clicked note is not the closest note that can be pressed.");
                }
            }
            else
            {
                //Debug.Log("No note detected in raycast.");
            }
        }
        else
        {
            Debug.Log("No note available to be hit.");
        }
    }

    // Helper method to convert HitAccuracy to NoteTiming
    private NoteTiming GetNoteTiming(HitAccuracy hitAccuracy)
    {
        switch (hitAccuracy)
        {
            case HitAccuracy.Early:
                return NoteTiming.Early;
            case HitAccuracy.Perfect:
                return NoteTiming.Perfect;
            case HitAccuracy.Late:
                return NoteTiming.Late;
            default:
                return NoteTiming.Miss;
        }
    }

    public void DisplayAndFadeText(TextMeshProUGUI text)
    {
        StartCoroutine(FadeTextRoutine(text));
    }

    private IEnumerator FadeTextRoutine(TextMeshProUGUI text)
    {
        CanvasGroup canvasGroup = text.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup component is missing on the feedback text.");
            yield break;
        }

        canvasGroup.alpha = 1f; // Set text to fully visible
        float displayDuration = 0.2f; // Display duration in seconds

        yield return new WaitForSeconds(displayDuration);

        // Gradually reduce the alpha to fade out
        float fadeDuration = 0.1f; // Fade-out duration
        float startAlpha = canvasGroup.alpha;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f; // Ensure it is fully invisible
    }

    public void RegisterNote(Note note)
    {
        if (activeNotes.Add(note))
        {
            //Debug.Log($"Registered note at beat {note.beatOfThisNote}");
        }
        else
        {
            Debug.LogWarning($"Failed to register note at beat {note.beatOfThisNote} (duplicate?)");
        }
    }

    public void DeregisterNote(Note note)
    {
        if (activeNotes.Remove(note))
        {
            //Debug.Log($"Deregistered note at beat {note.beatOfThisNote}");
        }
        else
        {
           // Debug.LogWarning($"Failed to deregister note at beat {note.beatOfThisNote} (not found)");
        }
    }

    /// <summary>
    /// Event handler for when a UI panel is opened.
    /// Disables input processing.
    /// </summary>
    private void OnUIOpened()
    {
        isUIActive = true;
    }

    /// <summary>
    /// Event handler for when a UI panel is closed.
    /// Enables input processing.
    /// </summary>
    private void OnUIClosed()
    {
        isUIActive = false;
    }

    void OnDestroy()
    {
        // **Unsubscribe from UIManager Events to Prevent Memory Leaks**
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIOpened -= OnUIOpened;
            UIManager.Instance.OnUIClosed -= OnUIClosed;
        }
    }
}

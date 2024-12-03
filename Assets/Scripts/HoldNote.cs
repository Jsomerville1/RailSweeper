// HoldNote.cs
using UnityEngine;

public class HoldNote : MonoBehaviour
{
    public float startBeat;
    public float endBeat;
    public Lane parentLane;

    private bool inHitZone = false;
    private bool isHolding = false;
    private bool isHoldCompleted = false;
    private float holdStartTime;
    private float expectedHoldDuration;
    private float holdProgress = 0f;

    // Material references
    private Material originalMaterial; // Initial material
    [SerializeField] private Material earlyMat;   // Early zone material
    [SerializeField] private Material perfectMat; // Perfect zone material
    [SerializeField] private Material lateMat;    // Late zone material
    [SerializeField] private Material holdingMat; // Material when the note is being held

    public NoteZone currentZone = NoteZone.None; // Current zone of the note
    private NoteZone holdStartZone = NoteZone.None;
    private bool isWaitingForRelease = false;    // Indicates we're waiting for the player to release the space bar
    private float waitStartTime;                 // Time when we started waiting for release
    private float maxReleaseTime;                // Maximum time allowed for release (half a beat)
    private bool canStartHold = true;            // Indicates if the player can start holding the note

    private float tickInterval;      // Time in seconds between ticks
    private float holdTickTimer;     // Time elapsed since the last hold tick
    private float tickBeatFraction = 0.25f; // Fraction of a beat per tick
    private float totalHoldTime = 0f;       // Total time held

    // Input lag
    private float inputLagInSeconds;

    // **Flag to track UI state**
    private bool isUIActive = false;

    void Start()
    {
        originalMaterial = GetComponent<Renderer>().material;
        InitializeHoldNote();

        // **Retrieve input lag directly from PlayerPrefs**
        float inputLagMs = PlayerPrefs.GetFloat("InputLag", 0f);
        inputLagInSeconds = inputLagMs / 1000f; // Convert milliseconds to seconds
        Debug.Log($"HoldNote: Input lag set to {inputLagInSeconds} seconds.");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIOpened += OnUIOpened;
            UIManager.Instance.OnUIClosed += OnUIClosed;
        }
    }

    void Update()
    {
        // **Do not process input if UI is active**
        if (isUIActive)
        {
            return;
        }

        if (inHitZone && !isHoldCompleted)
        {
            HandleInput();

            if (isHolding)
            {
                HandleHoldTick();
                holdProgress += Time.deltaTime;

                // Check if the player has held the note for the expected duration
                if (holdProgress >= expectedHoldDuration)
                {
                    isWaitingForRelease = true;
                    waitStartTime = Time.time;
                    isHolding = false; // Stop adding ticks
                    UpdateMaterialBasedOnZone();
                }
            }
            else if (isWaitingForRelease)
            {
                // Waiting for the player to release the key
                if (!Input.GetKey(KeyCode.Space))
                {
                    // Player released key in time
                    FinalizeHold();
                }
                else if ((Time.time - inputLagInSeconds) - waitStartTime >= maxReleaseTime)
                {
                    // Player failed to release in time
                    OnMiss();
                }
            }
        }
    }

    private void HandleHoldTick()
    {
        holdTickTimer += Time.deltaTime;
        if (holdTickTimer >= tickInterval)
        {
            holdTickTimer -= tickInterval;
            ScoreManager.Instance.AddHoldTickScore();
        }
    }

    public void InitializeHoldNote()
    {
        // Calculate expected hold duration in seconds
        expectedHoldDuration = (endBeat - startBeat) * SongManager.Instance.secPerBeat;
        maxReleaseTime = SongManager.Instance.secPerBeat * 1.5f; // Margin of error allowed for hold note successful hit (0.5 indicates half a beat)
        tickInterval = SongManager.Instance.secPerBeat * tickBeatFraction;

        // Set the scale based on the duration
        float length = parentLane.distanceBetweenNotes * (endBeat - startBeat);
        Vector3 scale = transform.localScale;
        scale.x = length;
        transform.localScale = scale;

        // Position the hold note correctly
        float centerBeat = (startBeat + endBeat) / 2;
        float xPos = -parentLane.distanceBetweenNotes * centerBeat;
        float adjust = SongManager.Instance.noteSpawnAdjuster + (SongManager.Instance.noteSpawnDelay * TrainController.Instance.speed);
        Vector3 position = new Vector3(xPos - adjust, transform.position.y, transform.position.z);
        transform.position = position;

        ResetNote();
    }


    // Handles player input for holding and releasing the space bar.
    private void HandleInput()
    {
        if (!isWaitingForRelease)
        {
            if (canStartHold)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    if (!isHolding)
                    {
                        StartHold();
                    }
                }
                else
                {
                    if (isHolding)
                    {
                        ReleaseHoldEarly();
                    }
                }
            }
            else
            {
                // Player cannot start hold because they were holding space when note entered
                if (!Input.GetKey(KeyCode.Space))
                {
                    canStartHold = true; // Allow them to start holding again after releasing
                }
            }
        }
    }

    // Initiates the hold when the player presses the space bar.
    private void StartHold()
    {
        isHolding = true;
        holdStartTime = Time.time;
        holdProgress = 0f;
        totalHoldTime = 0f;
        GetComponent<Renderer>().material = holdingMat;
        holdStartZone = currentZone;
        holdTickTimer = 0f; // Reset hold tick timer when hold starts
    }


    // Handles the early release of the hold.
    private void ReleaseHoldEarly()
    {
        isHolding = false;
        UpdateMaterialBasedOnZone();
        OnMiss();
    }

    // Finalizes the hold when the player successfully holds for the required duration.
    private void FinalizeHold()
    {
        isHoldCompleted = true;
        isWaitingForRelease = false;
        canStartHold = false; // Prevent starting a new hold until space is released

        // Adjust currentBeat for input lag
        float inputLagInBeats = inputLagInSeconds / SongManager.Instance.secPerBeat;
        float adjustedCurrentBeat = SongManager.Instance.currentBeat - inputLagInBeats;

        float offBeatDifference = adjustedCurrentBeat - endBeat;
        ScoreManager.Instance.Hit(NoteTiming.Perfect, offBeatDifference);

        // Play hit feedback based on the hold start zone
        if (MouseInputHandler.Instance != null)
        {
            switch (holdStartZone)
            {
                case NoteZone.Early:
                    MouseInputHandler.Instance.DisplayAndFadeText(MouseInputHandler.Instance.earlyText);
                    break;
                case NoteZone.Perfect:
                    MouseInputHandler.Instance.DisplayAndFadeText(MouseInputHandler.Instance.perfectText);
                    break;
                case NoteZone.Late:
                    MouseInputHandler.Instance.DisplayAndFadeText(MouseInputHandler.Instance.lateText);
                    break;
                default:
                    Debug.Log("Hold note completed with unknown zone.");
                    break;
            }
        }

        // Final score addition
        ScoreManager.Instance.HoldNoteComplete();

        // Return to pool
        HoldNotePool.Instance.ReturnObject(this.gameObject);
    }

    private void OnMiss()
    {
        isHoldCompleted = true;
        ScoreManager.Instance.Miss();

        if (MouseInputHandler.Instance != null)
        {
            MouseInputHandler.Instance.DisplayAndFadeText(MouseInputHandler.Instance.missText);
        }

        // Reset material
        GetComponent<Renderer>().material = originalMaterial;
        // Return to pool
        HoldNotePool.Instance.ReturnObject(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitZone"))
        {
            inHitZone = true;
            if (Input.GetKey(KeyCode.Space))
            {
                // Player is already holding space; cannot start hold
                canStartHold = false;
            }
            else
            {
                canStartHold = true;
            }
        }
        // Only change material if not currently being held
        if (!isHolding)
        {
            if (other.CompareTag("Early"))
            {
                GetComponent<Renderer>().material = earlyMat;
                currentZone = NoteZone.Early;
            }
            else if (other.CompareTag("Perfect"))
            {
                GetComponent<Renderer>().material = perfectMat;
                currentZone = NoteZone.Perfect;
            }
            else if (other.CompareTag("Late"))
            {
                GetComponent<Renderer>().material = lateMat;
                currentZone = NoteZone.Late;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HitZone"))
        {
            inHitZone = false;
            if (!isHoldCompleted)
            {
                OnMiss();
            }
        }
        // Only change material if not currently being held
        if (!isHolding)
        {
            if (other.CompareTag("Early") || other.CompareTag("Perfect") || other.CompareTag("Late"))
            {
                currentZone = NoteZone.None;
                GetComponent<Renderer>().material = originalMaterial;
            }
        }
    }


    // Updates the material based on the current zone.
    private void UpdateMaterialBasedOnZone()
    {
        switch (currentZone)
        {
            case NoteZone.Early:
                GetComponent<Renderer>().material = earlyMat;
                break;
            case NoteZone.Perfect:
                GetComponent<Renderer>().material = perfectMat;
                break;
            case NoteZone.Late:
                GetComponent<Renderer>().material = lateMat;
                break;
            default:
                GetComponent<Renderer>().material = originalMaterial;
                break;
        }
    }


    // Resets the note to its initial state. Useful when returning the note to the pool.
    public void ResetNote()
    {
        isHolding = false;
        isHoldCompleted = false;
        inHitZone = false;
        holdProgress = 0f;
        currentZone = NoteZone.None;
        holdStartZone = NoteZone.None;
        isWaitingForRelease = false;
        canStartHold = true;
        holdTickTimer = 0f;
        totalHoldTime = 0f;
        GetComponent<Renderer>().material = originalMaterial;
    }

    private void OnUIOpened()
    {
        isUIActive = true;

    }

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

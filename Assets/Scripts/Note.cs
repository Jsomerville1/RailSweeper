using System.Collections;
using UnityEngine;


/// <summary>
/// Represents a regular note in the rhythm game.
/// </summary>
public class Note : MonoBehaviour
{
 
    public float beatOfThisNote; // assigned by Melanchall plugin - the note's position in the MIDI file
    [HideInInspector]
    public Lane parentLane; // parent lane of the note
    [Header("Movement Settings")]
    public MovementDirection movementDirection; // direction of movement - assigned randomly from options set in the parent lane game object
    private float distanceMoved = 0f; // distance the note has moved
    public float movementSpeed; // Units per second
    public float movementDistance; // Total distance to move
    public Vector3 moveDirection; // vector of the of movement direction
    private Vector3 spawnPosition; // initial spawn position (based of beatOfThisNote)
    private Material originalMaterial; // Initial notem material (before it enters the hitzone)
    [SerializeField] private Material greenGlass; // perfect zone material - To be changed
    [SerializeField] private Material amberGlass; // early zone material - To be changed
    [SerializeField] private Material redGlass; // late zone material - To be changed
    [SerializeField] private Material clearGlass;  // Default material

    public bool canBePressed = false; // Indicates if the note is in the HitZone
    public GameSettings gameSettings; // Reference to GameSettings
    public NoteZone currentZone = NoteZone.None; // public var for current note zone - for Perfect/Early/Late text
    // Flash effect variables
    [SerializeField]
    private bool hasFlashed = false; // To ensure the note only flashes once
    private Renderer noteRenderer;   // Reference to the Renderer component

    void Awake()
    {
        // Find the GameSettings object in the scene
        gameSettings = GameSettings.Instance;
        if (gameSettings == null)
        {
            Debug.LogError("GameSettings not found in the scene. Please ensure there is a GameSettings object in the scene.");
        }
    
    }

    void Start()
    {
        InitializeMovement();
        noteRenderer = GetComponent<Renderer>();
        if (noteRenderer != null)
        {
            // Set the note's initial material to clearGlass
            noteRenderer.material = clearGlass;
            noteRenderer.material.EnableKeyword("_EMISSION"); // Enable emission
        }
        else
        {
            Debug.LogError("Note: Renderer component not found.");
        }
    }
    void Update()
    {
        if (LevelEndUI.Instance != null && LevelEndUI.Instance.levelEnded)
        return;
        MoveNote();

        // Check if it's time to flash
        CheckForFlash();
    }

    /// <summary>
    /// Coroutine to handle the flash effect.
    /// </summary>
    IEnumerator FlashNote()
    {
        if (noteRenderer != null)
        {
            // Store the current material and emission color
            Material currentMaterial = noteRenderer.material;
            Color originalEmissionColor = currentMaterial.GetColor("_EmissionColor");

            // Set the emission color to a brighter color (adjust the color and multiplier as needed)
            Color flashColor = Color.green * 2.8f; // Multiplier increases brightness
            currentMaterial.SetColor("_EmissionColor", flashColor);

            // Wait for a short duration (e.g., 0.2 seconds)
            yield return new WaitForSeconds(0.2f);

            // Revert to the appropriate material based on the current zone
            UpdateMaterialBasedOnCurrentZone();
        }
        else
        {
            Debug.Log("Note renderer is null");
        }
    }


    /// <summary>
    /// Checks if the note should flash based on the song's current beat.
    /// </summary>
    void CheckForFlash()
    {
        if (hasFlashed)
            return; // Already flashed

        float currentBeat = SongManager.Instance.GetSongPositionInBeats();
        float epsilon = 0.05f; // Tolerance for beat matching (adjust as needed)

        if (Mathf.Abs(currentBeat - beatOfThisNote) <= epsilon)
        {
            // Start the flash coroutine
            StartCoroutine(FlashNote());
            hasFlashed = true;
        }
    }

    void UpdateMaterialBasedOnCurrentZone()
    {
        switch (currentZone)
        {
            case NoteZone.Early:
                noteRenderer.material = amberGlass;
                break;
            case NoteZone.Perfect:
                noteRenderer.material = greenGlass;
                break;
            case NoteZone.Late:
                noteRenderer.material = redGlass;
                break;
            default:
                noteRenderer.material = clearGlass;
                break;
        }
        noteRenderer.material.EnableKeyword("_EMISSION"); // Ensure emission is enabled
    }

    /// <summary>
    /// Sets movement parameters based on current combo. Assists 
    /// </summary>
    public void AdjustMovementParameters()
    {
        if (gameSettings == null)
        {
            Debug.LogError("GameSettings reference is null in Note. Cannot adjust movement parameters.");
            return;
        }

        int combo = ScoreManager.Instance.currentCombo;
        int highestCombo = ScoreManager.Instance.highestCombo;
        int notesSinceMiss = ScoreManager.Instance.notesHitSinceMiss;
        int effectiveCombo;

        if (combo == 0 && highestCombo > 0)
        {
            // Smoothly ramp up difficulty over 'catchUpDuration' notes
            float t = Mathf.Clamp01((float)notesSinceMiss / gameSettings.catchUpDuration);
            effectiveCombo = Mathf.RoundToInt(Mathf.Lerp(0, highestCombo, t));
        }
        else
        {
            effectiveCombo = combo;
        }


        // Calculate multipliers based on effective combo and game settings
        float speedMultiplier = 1f + (gameSettings.speedIncrementPerHit * effectiveCombo);
        speedMultiplier = Mathf.Min(speedMultiplier, gameSettings.maxSpeedMultiplier);

        float distanceMultiplier = 1f + (gameSettings.movementIncrementPerHit * effectiveCombo);
        distanceMultiplier = Mathf.Min(distanceMultiplier, gameSettings.maxMovementMultiplier);

        // Set movement speed and distance
        movementSpeed = gameSettings.defaultMovementSpeed * speedMultiplier;
        movementDistance = gameSettings.defaultMovementDistance * distanceMultiplier;

        // Clamp movementDistance within allowed limits
        movementDistance = Mathf.Clamp(movementDistance, gameSettings.minMovementDistance, gameSettings.maxMovementDistance);
    }



    /// <summary>
    /// Initializes the movement direction based on the assigned MovementDirection enum.
    /// </summary>
    public void InitializeMovement()
    {
        spawnPosition = transform.position;
        moveDirection = GetDirectionVector(movementDirection);
    }

    /// <summary>
    /// Called when the note is successfully hit.
    /// </summary>
    public void OnHit(NoteTiming timing, float offBeatDifference)
    {
        // Deregister from MouseInputHandler
        if (MouseInputHandler.Instance != null)
        {
            MouseInputHandler.Instance.DeregisterNote(this);
        }

        // Register the hit with timing and offBeatDifference
        ScoreManager.Instance.Hit(timing, offBeatDifference);

        // Notify parent lane
        if (parentLane != null)
        {
            parentLane.RemoveNoteFromList(this);
        }
        // Return the note to the object pool
        ObjectPool.Instance.ReturnObject(this.gameObject);
    }

    public void OnMiss()
    {
        // Register the miss
        ScoreManager.Instance.Miss();

        // Deregister from MouseInputHandler
        if (MouseInputHandler.Instance != null)
        {
            MouseInputHandler.Instance.DeregisterNote(this);
        }

        // Notify parent lane
        if (parentLane != null)
        {
            parentLane.RemoveNoteFromList(this);
        }

        //Debug.Log($"Note at beat {beatOfThisNote} was missed.");

        // Return the note to the object pool
        //ObjectPool.Instance.ReturnObject(this.gameObject);
    }
    // Kept in case of old references. Not sure if needed ******
    public void OnHit()
    {
        OnHit(NoteTiming.Perfect, 0f);
    }

    /// <summary>
    /// Moves the note in the assigned direction at the specified speed.
    /// </summary>
    private void MoveNote()
    {
        if (moveDirection == Vector3.zero)
        {
            return;
        }

        // Adjust speed and distance based on combo and game difficulty
        //AdjustMovementBasedOnCombo();

        float delta = movementSpeed * Time.deltaTime;
        transform.Translate(moveDirection * delta, Space.World);
        distanceMoved += delta;

        if (distanceMoved >= movementDistance /*|| transform.position.y <= 5.0f*/)
        {
            distanceMoved = 0;
            ReverseMovementDirection();
        }

        // Reverse direction if moving downward and Y <= 5 //////
        if (moveDirection.y < 0 && transform.position.y <= 5f)
        {
            // Ensure the note doesn't go below Y=5
            Vector3 pos = transform.position;
            pos.y = 5f;
            transform.position = pos;

            // Reverse the movement direction
            ReverseMovementDirection();
        }
        // Safety clamp to prevent Y from going below 5 /////
        if (transform.position.y < 5f)
        {
            Vector3 pos = transform.position;
            pos.y = 5f;
            transform.position = pos;
        }


    }

    /// <summary>
    /// Reverses the movement direction once the note reaches the movement distance limit.
    /// </summary>
    private void ReverseMovementDirection()
    {
        moveDirection = -moveDirection;
    }
private Vector3 GetDirectionVector(MovementDirection direction)
{
    switch (direction)
    {
        case MovementDirection.Up:
            return Vector3.up;
        case MovementDirection.Down:
            return Vector3.down;
        case MovementDirection.Left:
            return Vector3.back;
        case MovementDirection.Right:
            return Vector3.forward;
        case MovementDirection.UpRight:
            return (Vector3.up + Vector3.forward).normalized;
        case MovementDirection.UpLeft:
            return (Vector3.up + Vector3.back).normalized;
        case MovementDirection.DownRight:
            return (Vector3.down + Vector3.forward).normalized;
        case MovementDirection.DownLeft:
            return (Vector3.down + Vector3.back).normalized;
        case MovementDirection.None:
        default:
            return Vector3.zero;
    }
}
    /// <summary>
    /// Detects when the note enters the HitZone.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // Check for "HitZone" and set note canBePressed to true
        if (other.CompareTag("HitZone"))
        {
            canBePressed = true;
            //Debug.Log($"Note FOR beat {beatOfThisNote} entered HitZone AT beat {SongManager.Instance.currentBeat}."); // TIMING DEBUG
        }
        // Check for the "Early" zone
        if (other.CompareTag("Early"))
        {
            // set the material
            GetComponent<Renderer>().material = amberGlass;
            // set the zone identifier
            currentZone = NoteZone.Early;
            //Debug.Log($"Note FOR beat {beatOfThisNote} entered Early zone at beat {SongManager.Instance.currentBeat}."); // TIMING DEBUG
        }
        // Check for the "Perfect" zone
        else if (other.CompareTag("Perfect"))
        {
            // set the material
            GetComponent<Renderer>().material = greenGlass;
            // set the zone identifier
            currentZone = NoteZone.Perfect;
            Debug.Log($"Note FOR beat {beatOfThisNote} entered Perfect zone at beat {SongManager.Instance.currentBeat}."); // TIMING DEBUG
        }
        // Check for the "Late" zone
        else if (other.CompareTag("Late"))
        {
            // set the material
            GetComponent<Renderer>().material = redGlass;

            // set the zone identifier
            currentZone = NoteZone.Late;
            //Debug.Log($"Note FOR beat {beatOfThisNote} entered Late zone at beat {SongManager.Instance.currentBeat}."); // TIMING DEBUG
        }

    }


    /// <summary>
    /// Detects when the note exits the HitZone without being hit.
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HitZone"))
        {
            canBePressed = false;

            // Register a miss if the note exits the HitZone without being hit
            if (parentLane != null)
            {
                parentLane.RemoveNoteFromList(this);
                parentLane.RegisterMiss();
                
            }

            Debug.Log($"Note at beat {beatOfThisNote} exited HitZone without being hit.");

            // Return the note to the object pool
            ObjectPool.Instance.ReturnObject(this.gameObject);
        }
/*
        // reset currentZone when exiting hitzone
        if (other.CompareTag("Early") || other.CompareTag("Perfect") || other.CompareTag("Late"))
        {
            currentZone = NoteZone.None;
        }
*/
    }

    /// <summary>
    /// Resets the note to its initial state. Useful if using object pooling.
    /// </summary>
    public void ResetNote()
    {
        noteRenderer.material = clearGlass;
        noteRenderer.material.EnableKeyword("_EMISSION"); // Ensure emission is enabled
        canBePressed = false;
        distanceMoved = 0f;
        moveDirection = Vector3.zero;
        movementDirection = MovementDirection.None;
        transform.position = spawnPosition;
        gameObject.SetActive(true);
        currentZone = NoteZone.None;
        hasFlashed = false;
    }

    public void RegisterWithMouseInputHandler()
    {
        if (MouseInputHandler.Instance != null)
        {
            MouseInputHandler.Instance.RegisterNote(this);
            //Debug.Log($"Note at beat {beatOfThisNote} registered with MouseInputHandler."); // DEBUG
        }
    }

    void OnDisable()
    {
        // Deregister this note
        if (MouseInputHandler.Instance != null)
        {
            MouseInputHandler.Instance.DeregisterNote(this);
            //Debug.Log($"Note at beat {beatOfThisNote} deregistered from MouseInputHandler."); // DEBUG

        }
    } 



#if UNITY_EDITOR
    /// <summary>
    /// Visualizes the note's position in the editor.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (parentLane == null || parentLane.hitZoneTransform == null)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
#endif
}

using UnityEngine;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;

public enum LaneType
{
    Regular,
    Hold
}

public class HoldNoteTimeStamp
{
    public float startBeat;
    public float endBeat;

    public HoldNoteTimeStamp(float startBeat, float endBeat)
    {
        this.startBeat = startBeat;
        this.endBeat = endBeat;
    }
}

public class Lane : MonoBehaviour
{
    [Header("Lane Settings")]
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;
    public LaneType laneType;

    private List<Note> notes = new List<Note>();
    public List<float> timeStamps = new List<float>(); // Beat positions for regular notes
    private int nextIndex = 0;

    [Header("Lane Transforms")]
    public Transform hitZoneTransform;

    [HideInInspector]
    public float distanceBetweenNotes;
    [HideInInspector]
    public float beatsShownInAdvance;

    [Header("Movement Directions")]
    public float songPositionInBeats;
    private List<HoldNoteTimeStamp> holdNoteTimeStamps = new List<HoldNoteTimeStamp>();
    private int nextHoldNoteIndex = 0;

    private static MovementPatternGenerator sharedMovementPatternGenerator;
    private static MovementGraph sharedGraph;

    void Start()
    {

        Debug.Log($"Lane '{gameObject.name}' Start called.");

        // Ensure hitZoneTransform is assigned
        if (hitZoneTransform == null)
        {
            Debug.LogError("Lane: HitZone Transform is not assigned.");
        }

        // Initialize the shared graph and movement pattern generator
        if (sharedGraph == null)
        {
            sharedGraph = new MovementGraph();
        }

        if (sharedMovementPatternGenerator == null)
        {
            sharedMovementPatternGenerator = new MovementPatternGenerator(sharedGraph);
        }
    }

    /// Initializes lane parameters.
    public void InitializeLane(float distanceBetweenNotes, float beatsShownInAdvance)
    {
        this.distanceBetweenNotes = distanceBetweenNotes;
        this.beatsShownInAdvance = beatsShownInAdvance;
        Debug.Log($"Lane '{gameObject.name}': Initialized with DistanceBetweenNotes = {distanceBetweenNotes}, BeatsShownInAdvance = {beatsShownInAdvance}");
    }

    /// Sets the time stamps based on MIDI notes.
    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        float epsilon = 0.0001f; // Tolerance for comparing floating-point numbers

        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction)
            {
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.Instance.midiFile.GetTempoMap());
                float timeInSeconds = (float)metricTimeSpan.TotalSeconds - SongManager.Instance.firstBeatOffset;
                float timeInBeats = timeInSeconds / SongManager.Instance.secPerBeat;

                bool isDuplicate = false;

                if (laneType == LaneType.Hold)
                {
                    // It's a hold note
                    float durationInSeconds = (float)note.LengthAs<MetricTimeSpan>(SongManager.Instance.midiFile.GetTempoMap()).TotalSeconds;
                    float durationInBeats = durationInSeconds / SongManager.Instance.secPerBeat;
                    float endBeat = timeInBeats + durationInBeats;

                    // Check for duplicates in holdNoteTimeStamps
                    foreach (var existingHoldNote in holdNoteTimeStamps)
                    {
                        if (Mathf.Abs(existingHoldNote.startBeat - timeInBeats) < epsilon)
                        {
                            isDuplicate = true;
                            break;
                        }
                    }

                    if (!isDuplicate)
                    {
                        holdNoteTimeStamps.Add(new HoldNoteTimeStamp(timeInBeats, endBeat));
                    }
                    else
                    {
                        Debug.Log($"Duplicate hold note found at beat {timeInBeats}. Skipping.");
                    }
                }
                else if (laneType == LaneType.Regular)
                {
                    // Regular note
                    // Check for duplicates in timeStamps
                    foreach (var existingTime in timeStamps)
                    {
                        if (Mathf.Abs(existingTime - timeInBeats) < epsilon)
                        {
                            isDuplicate = true;
                            break;
                        }
                    }

                    if (!isDuplicate)
                    {
                        timeStamps.Add(timeInBeats);
                    }
                    else
                    {
                        Debug.Log($"Duplicate note found at beat {timeInBeats}. Skipping.");
                    }
                }
            }
                //Debug.Log($"Lane '{gameObject.name}': SetTimeStamps added {timeStamps.Count} regular beats and {holdNoteTimeStamps.Count} hold notes after removing duplicates.");

        }

        // Sort both lists
        timeStamps.Sort();
        holdNoteTimeStamps.Sort((a, b) => a.startBeat.CompareTo(b.startBeat));

        //Debug.Log($"Lane '{gameObject.name}': SetTimeStamps added {timeStamps.Count} beats after removing duplicates.");
    }

    void Update()
    {
        // Check if note spawning is allowed
        if (!SongManager.Instance.CanSpawnNotes() || SongManager.Instance.IsPaused())
        {
            return; // If notes can't be spawned yet, exit early
        }

        songPositionInBeats = SongManager.Instance.GetSongPositionInBeats();

        // Spawn notes that are within the upcoming beatsShownInAdvance
        while (nextIndex < timeStamps.Count && timeStamps[nextIndex] < songPositionInBeats + beatsShownInAdvance)
        {
            SpawnNoteAtBeat(timeStamps[nextIndex]);
            nextIndex++;
        }

        // Spawn hold notes
        while (nextHoldNoteIndex < holdNoteTimeStamps.Count && holdNoteTimeStamps[nextHoldNoteIndex].startBeat < songPositionInBeats + beatsShownInAdvance)
        {
            SpawnHoldNoteAtBeat(holdNoteTimeStamps[nextHoldNoteIndex]);
            nextHoldNoteIndex++;
        }
    }


    // Spawns a note at a specific beat position.
    void SpawnNoteAtBeat(float beat)
    {
        float yOffset = 5.0f; // Adjust this value as needed to control the height
        float adjust = SongManager.Instance.noteSpawnAdjuster + (SongManager.Instance.noteSpawnDelay * TrainController.Instance.speed);
        // Calculate the spawn position based on the HitZone's position and beat timing
        Vector3 spawnPosition = new Vector3(-distanceBetweenNotes * beat, yOffset, 0f); // No dependency on hitZoneTransform
        spawnPosition.x -= adjust;
        spawnPosition.y += yOffset;

        // Ensure the Y position is at least 5
        spawnPosition.y = Mathf.Max(spawnPosition.y, 5f);

        // Retrieve a note from the object pool
        GameObject noteObject = ObjectPool.Instance.GetObject();
        if (noteObject == null)
        {
            Debug.LogWarning($"Lane '{gameObject.name}': Unable to spawn note. Object pool is exhausted.");
            return;
        }

        // Set the note's position without parenting it
        noteObject.transform.SetParent(null, true); // No parent, keep world position
        noteObject.transform.position = spawnPosition;
        noteObject.transform.rotation = Quaternion.identity;

        Note noteComponent = noteObject.GetComponent<Note>();

        if (noteComponent != null)
        {
            // Assign parentLane
            noteComponent.parentLane = this;

            // Assign beatOfThisNote
            noteComponent.beatOfThisNote = beat;

            // Now that beatOfThisNote is assigned, register the note
            noteComponent.RegisterWithMouseInputHandler();

            // Assign a random movement direction from availableDirections
            AssignMovementDirection(noteComponent);
            noteComponent.AdjustMovementParameters();

            // Call InitializeMovement after setting movementDirection
            noteComponent.InitializeMovement();

            // DEBUG ****
            if (DebugManager.Instance != null)
            {
                DebugManager.Instance.UpdateDebugText(
                    $"Note Beat: {beat}\n" +
                    $"Speed: {noteComponent.movementSpeed:F2}\n" +
                    $"Distance: {noteComponent.movementDistance:F2}\n" +
                    $"Direction: {noteComponent.movementDirection}\n" +
                    $"Difficulty: {GameSettings.Instance.difficulty}"
                );
            }



            // Add note to the list for hit detection
            notes.Add(noteComponent);
        }
        else
        {
            Debug.LogError($"Lane '{gameObject.name}': Note component not found on the note prefab.");
            ObjectPool.Instance.ReturnObject(noteObject); // Return to pool since it's invalid
        }
    }

    void SpawnHoldNoteAtBeat(HoldNoteTimeStamp holdNoteTimeStamp)
    {
        Debug.Log($"Spawning hold note at beat {holdNoteTimeStamp.startBeat}.");

        // Similar to SpawnNoteAtBeat, but for hold notes
        float yOffset = 2.5f;
        float adjust = SongManager.Instance.noteSpawnAdjuster + (SongManager.Instance.noteSpawnDelay * TrainController.Instance.speed);

        Vector3 spawnPosition = new Vector3(-distanceBetweenNotes * holdNoteTimeStamp.startBeat, yOffset, 0f);
        spawnPosition.x -= adjust;
        spawnPosition.y += yOffset;

        GameObject holdNoteObject = HoldNotePool.Instance.GetObject();
        if (holdNoteObject == null)
        {
            Debug.LogWarning("Unable to spawn hold note.");
            return;
        }

        holdNoteObject.transform.SetParent(null, true);
        holdNoteObject.transform.position = spawnPosition;
        holdNoteObject.transform.rotation = Quaternion.identity;

        HoldNote holdNoteComponent = holdNoteObject.GetComponent<HoldNote>();

        if (holdNoteComponent != null)
        {
            holdNoteComponent.parentLane = this;
            holdNoteComponent.startBeat = holdNoteTimeStamp.startBeat;
            holdNoteComponent.endBeat = holdNoteTimeStamp.endBeat;

            holdNoteComponent.InitializeHoldNote();
        }
        else
        {
            Debug.LogError("HoldNote component not found on the prefab.");
            HoldNotePool.Instance.ReturnObject(holdNoteObject);
        }
    }


    // Assigns a random movement direction to the note from the availableDirections array.
    void AssignMovementDirection(Note note)
    {
        note.movementDirection = sharedMovementPatternGenerator.GetNextDirection();
        note.InitializeMovement();
    }


    // Removes a note from the lane's list.
    public void RemoveNoteFromList(Note note)
    {
        if (notes.Contains(note))
        {
            notes.Remove(note);
        }
    }

    // Registers a miss for a note.
    public void RegisterMiss()
    {
        ScoreManager.Instance.Miss();
    }
}

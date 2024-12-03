using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class TrainController : MonoBehaviour
{
    public float speed; // Calculated based on BPM and DistancePerBeat
    // Singleton instance
    public static TrainController Instance { get; private set; }

    public Transform trainTransform;
    public Camera mainCamera;


    void Awake()
    {
        // Ensure that only one instance of TrainController exists
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        
        // Ensure the trainTransform is set
        if (trainTransform == null)
        {
            trainTransform = this.transform; // Defaults to this object's transform if not set
        }
        // Ensure the mainCamera is assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("TrainController: Main Camera is not assigned and no Camera with 'MainCamera' tag found.");
            }
        }
    }



    // Sets the train's speed based on distanceBetweenNotes and BPM.
    public void SetSpeed(float distanceBetweenNotes, float bpm)
    {
        if (bpm <= 0f || distanceBetweenNotes <= 0f)
        {
            Debug.LogError("TrainController: Invalid BPM or DistanceBetweenNotes.");
            speed = 0f;
            return;
        }

        // Speed = distanceBetweenNotes * (BPM / 60)
        speed = distanceBetweenNotes * (bpm / 60f);
        Debug.Log($"TrainController: Calculated speed = {speed} units/sec based on BPM = {bpm}, DistanceBetweenNotes = {distanceBetweenNotes}");
    }

    void Update()
    {
        if (speed <= 0f)
        {
            // Prevent movement if speed is not set correctly
            return;
        }

        // Move the train forward along the x axis
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        //Debug.Log($"TrainController: Moving forward. Current Position: {transform.position}");
    }

    void OnDrawGizmos()
    {
        // Draw a forward line to visualize movement direction
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * 5f);
    }
}

// ObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a pool of reusable GameObjects to optimize performance.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [Header("Pool Settings")]
    public GameObject objectPrefab; // Assign Note prefab here
    public int poolSize = 1100; // Initial pool size

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        InitializePool();
    }

    /// <summary>
    /// Initializes the object pool by pre-instantiating objects.
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(objectPrefab);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
        Debug.Log($"ObjectPool: Initialized with {poolSize} objects."); // DEBUG

    }

    /// <summary>
    /// Retrieves an object from the pool.
    /// </summary>
    /// <returns>A pooled GameObject.</returns>
    public GameObject GetObject()
    {
        if (poolQueue.Count > 0)
        {
            GameObject obj = poolQueue.Dequeue();
            obj.SetActive(true);
            //Debug.Log($"Obj. pool Obj retreived: Count = {poolQueue.Count}"); // debug
            return obj;
        }
        else
        {
            Debug.LogWarning("Obj. pool exhaust. No more available.");
            return null; // pools closed
        }
    }

    /// <summary>
    /// Returns an object to the pool.
    /// </summary>
    /// <param name="obj">GameObject to return.</param>
    public void ReturnObject(GameObject obj)
    {
        // Ensure the object is indeed part of this pool
        if (obj == null)
        {
            Debug.LogWarning("ObjectPool: Attempted to return a null object."); // DEBUG
            return;
        }

        Note note = obj.GetComponent<Note>();
        if (note != null)
        {
            note.ResetNote(); // Reset the note's state
        }
        else
        {
            Debug.LogWarning("ObjectPool: Returned object does not have a Note component."); // DEBUG
        }

        obj.SetActive(false);
        poolQueue.Enqueue(obj);
        //Debug.Log($"ObjectPool: Returned object. Pool Count: {poolQueue.Count}"); // DEBUG
    }


}
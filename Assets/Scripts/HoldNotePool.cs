// HoldNotePool.cs
using UnityEngine;
using System.Collections.Generic;

public class HoldNotePool : MonoBehaviour
{
    public static HoldNotePool Instance;

    public GameObject holdNotePrefab;
    public int poolSize = 100;

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(holdNotePrefab);
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        if (poolQueue.Count > 0)
        {
            GameObject obj = poolQueue.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            Debug.LogWarning("HoldNotePool exhausted.");
            return null;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        HoldNote holdNote = obj.GetComponent<HoldNote>();
        if (holdNote != null)
        {
            holdNote.ResetNote();
        }

        obj.SetActive(false);
        poolQueue.Enqueue(obj);
    }
}

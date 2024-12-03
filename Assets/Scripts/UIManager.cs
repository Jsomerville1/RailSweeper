// UIManager.cs
using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // Tracks the number of active UI panels
    private int activeUIPanels = 0;

    // **Events for UI State Changes**
    public event Action OnUIOpened;
    public event Action OnUIClosed;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes if needed
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Call this method when a UI panel is opened.
    /// </summary>
    public void OpenUIPanel()
    {
        activeUIPanels++;
        Debug.Log($"UI Panel Opened. Active Panels: {activeUIPanels}");
        UpdateCursorState();

        // **Invoke Event**
        OnUIOpened?.Invoke();
    }

    /// <summary>
    /// Call this method when a UI panel is closed.
    /// </summary>
    public void CloseUIPanel()
    {
        activeUIPanels = Mathf.Max(activeUIPanels - 1, 0);
        Debug.Log($"UI Panel Closed. Active Panels: {activeUIPanels}");
        UpdateCursorState();

        // **Invoke Event**
        OnUIClosed?.Invoke();
    }

    /// <summary>
    /// Determines whether any UI panel is currently active.
    /// </summary>
    public bool IsAnyUIPanelActive()
    {
        return activeUIPanels > 0;
    }

    /// <summary>
    /// Updates the cursor visibility and lock state based on UI activity.
    /// </summary>
    private void UpdateCursorState()
    {
        if (IsAnyUIPanelActive())
        {
            // Show and unlock the cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // Hide and lock the cursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}

using UnityEngine;
using TMPro;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    public TextMeshProUGUI debugText;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void UpdateDebugText(string text)
    {
        if (debugText != null)
        {
            debugText.text = text;
        }
    }
}

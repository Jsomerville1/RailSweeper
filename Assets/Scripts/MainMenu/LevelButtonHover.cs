
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Assign the sample audio clip for this level.")]
    public AudioClip sampleClip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Notify the LevelSelectAudioManager to play the sample clip
        LevelSelectAudioManager.Instance.PlaySampleClip(sampleClip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Notify the LevelSelectAudioManager to stop the sample clip
        LevelSelectAudioManager.Instance.StopSampleClip();
    }
}

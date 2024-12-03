using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MascotClickHandler : MonoBehaviour, IPointerClickHandler
{
    private Animator animator;
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip jiggleSound;

    void Start()
    {
        // Get the Animator component attached to the mascot image
        animator = GetComponent<Animator>();

        // Add an AudioSource component to play the sound
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = jiggleSound;
    }

    // Implement the OnPointerClick interface method
    public void OnPointerClick(PointerEventData eventData)
    {
        // Play the jiggle animation from the beginning
        if (animator != null)
        {
            animator.Play("MascotJiggle", -1, 0f);
        }

        // Play the jiggle sound
        if (audioSource != null && jiggleSound != null)
        {
            audioSource.Play();
        }
    }
}

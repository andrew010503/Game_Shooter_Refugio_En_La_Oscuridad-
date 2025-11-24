using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [Header("Hover Sound")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;

    [Header("Audio Source (Optional)")]
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound();
    }

    private void PlayHoverSound()
    {
        if (hoverSound == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(hoverSound, Camera.main.transform.position, volume);
        }
    }
}

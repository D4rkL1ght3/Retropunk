using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Explosion Audio")]
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();

            Destroy(
                gameObject,
                audioSource.clip.length
            );
        }
        else
        {
            // Fallback in case no audio is assigned.
            Destroy(gameObject);
        }
    }
}
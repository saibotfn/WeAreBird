using UnityEngine;

public class SoundZone : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clip;
    public bool playOnlyOnce = true;

    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playOnlyOnce && hasPlayed) return;

        source.PlayOneShot(clip);
        hasPlayed = true;
    }
}
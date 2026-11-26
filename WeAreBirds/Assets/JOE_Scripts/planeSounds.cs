using UnityEngine;

public class PlaneSounds : MonoBehaviour
{
    public AudioSource source;
    public AudioClip landingSound;
    public AudioClip takeoffSound;

    public void PlayLanding() => source.PlayOneShot(landingSound);
    public void PlayTakeoff() => source.PlayOneShot(takeoffSound);
}
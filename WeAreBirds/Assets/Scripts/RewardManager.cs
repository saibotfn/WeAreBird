using UnityEngine;

public class RewardManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public AudioSource audioSource;
   public HapticImpulsePlayer hapticImpulsePlayer;
     void OnTriggerEnter(Collider other) 
     {
        audioSource.Play();
        hapticImpulsePlayer.Play();


     }
}

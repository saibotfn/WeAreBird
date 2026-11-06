using UnityEngine;

public class RewardManager : MonoBehaviour

{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public AudioSource audioSource;
   public HapticImpulsePlayer hapticImpulsePlayer;
   
    void Start
    {
        var impulse = (args.interactorObject as Component) .GetComponentInParent<HapticImpulsePlayer>(); // strength, seconds
    }

     void OnTriggerEnter(Collider other) 
     {
        audioSource.Play();
        impulse.SendHapticImpulse(0.6f, 0.1f);   

     }
}

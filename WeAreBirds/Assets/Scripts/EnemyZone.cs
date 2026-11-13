using UnityEngine;

public class EnemyZone : MonoBehaviour
{
    public bool playerInside = false;
    public Transform player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            player = other.transform;

            Debug.Log("Player entered the enemy zone!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            player = null;

            Debug.Log("Player left the enemy zone.");
        }
    }
}

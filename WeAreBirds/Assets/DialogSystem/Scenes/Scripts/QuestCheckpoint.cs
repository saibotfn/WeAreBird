
using UnityEngine;

public class QuestCheckpoint : MonoBehaviour
{
    [Header("Quest Settings")]
    public QuestStage requiredStage;
    public QuestStage nextStage;

    [Header("Detection Settings")]
    public Transform player;
    public float detectionRadius = 3f;

    void Update()
    {
        if (player == null ||
            QuestManager.Instance == null)
            return;

        if (QuestManager.Instance.currentStage
            != requiredStage)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= detectionRadius)
        {
            QuestManager.Instance.SetStage(nextStage);

            Debug.Log("Checkpoint reached: " + gameObject.name);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius
        );
    }
}

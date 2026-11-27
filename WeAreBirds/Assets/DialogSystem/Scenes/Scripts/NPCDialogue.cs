
using UnityEngine;

[System.Serializable]
public class QuestDialogue
{
    public QuestStage requiredStage;

    [TextArea(2, 8)]
    public string[] dialogueLines;

    public bool advanceQuestOnCompletion;
    public QuestStage nextStage;
}

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC Information")]
    public string npcName;

    [Header("Quest Dialogues")]
    public QuestDialogue[] questDialogues;

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public Transform player;
    public DialogueManager dialogueManager;

    private bool playerWasInRange = false;

    void Update()
    {
        if (player == null || dialogueManager == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        bool playerInRange =
            distance <= interactionDistance;

        if (playerInRange && !playerWasInRange)
        {
            dialogueManager.StartDialogue(this);
        }

        if (!playerInRange && playerWasInRange)
        {
            dialogueManager.EndDialogue(this);
        }

        playerWasInRange = playerInRange;
    }

    public QuestDialogue GetCurrentDialogue()
    {
        if (QuestManager.Instance == null ||
            questDialogues == null)
            return null;

        foreach (QuestDialogue dialogue in questDialogues)
        {
            if (dialogue.requiredStage ==
                QuestManager.Instance.currentStage)
            {
                return dialogue;
            }
        }

        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionDistance
        );
    }
}

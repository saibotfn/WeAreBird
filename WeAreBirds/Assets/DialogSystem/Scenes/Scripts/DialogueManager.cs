
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text npcNameText;
    public TMP_Text dialogueText;

    private NPCDialogue activeNPC;
    private QuestDialogue activeDialogue;

    private string[] dialogueLines;
    private int currentLine = 0;
    private bool dialogueActive = false;

    InputAction nextButton;

    void Awake()
    {
        dialoguePanel.SetActive(false);
        nextButton = new InputAction(type: InputActionType.Button);
        nextButton.AddBinding("<XRController>{RightHand}/primaryButton"); // A on Quest
        nextButton.AddBinding("<Keyboard>/space");                        // for testing
        nextButton.Enable();
    }

    void Update()
    {

        if (nextButton.WasPressedThisFrame() && dialogueActive)
        {
            NextLine();
        }
    }

    public void StartDialogue(NPCDialogue npc)
    {
        if (npc == null || dialogueActive)
            return;

        QuestDialogue selectedDialogue =
            npc.GetCurrentDialogue();

        if (selectedDialogue == null ||
            selectedDialogue.dialogueLines == null ||
            selectedDialogue.dialogueLines.Length == 0)
            return;

        activeNPC = npc;
        activeDialogue = selectedDialogue;

        dialogueLines = selectedDialogue.dialogueLines;

        currentLine = 0;
        dialogueActive = true;

        dialoguePanel.SetActive(true);
        npcNameText.text = npc.npcName;

        ShowLine();
    }

    public void NextLine()
    {
        if (!dialogueActive)
            return;

        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            CompleteDialogue();
        }
        else
        {
            ShowLine();
        }
    }

    private void ShowLine()
    {
        dialogueText.text = dialogueLines[currentLine];
    }

    private void CompleteDialogue()
    {
        if (activeDialogue != null &&
            activeDialogue.advanceQuestOnCompletion &&
            QuestManager.Instance != null)
        {
            QuestManager.Instance.SetStage(
                activeDialogue.nextStage
            );
        }

        EndDialogue();
    }

    public void EndDialogue(NPCDialogue npc)
    {
        if (activeNPC == npc)
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        dialogueActive = false;
        activeNPC = null;
        activeDialogue = null;

        dialoguePanel.SetActive(false);
    }
}

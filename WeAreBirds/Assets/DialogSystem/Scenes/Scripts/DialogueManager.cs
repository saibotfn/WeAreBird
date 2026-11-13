
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text npcNameText;
    public TMP_Text dialogueText;

    [Header("Test NPC (Without VR)")]
    public NPCDialogue testNPC;

    private string[] dialogueLines;
    private int currentLine = 0;
    private bool dialogueActive = false;

    void Start()
    {
        // Hide dialogue when the game starts
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Automatically start dialogue for testing
        if (testNPC != null)
        {
            StartDialogue(testNPC);
        }
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // SPACE: Continue to next dialogue line
        if (Keyboard.current.spaceKey.wasPressedThisFrame
            && dialogueActive)
        {
            NextLine();
        }

        // E: Restart test NPC dialogue
        if (Keyboard.current.eKey.wasPressedThisFrame
            && !dialogueActive
            && testNPC != null)
        {
            StartDialogue(testNPC);
        }
    }

    // Start dialogue with a specific NPC
    public void StartDialogue(NPCDialogue npc)
    {
        if (npc == null)
        {
            Debug.LogWarning("No NPC assigned!");
            return;
        }

        if (dialoguePanel == null ||
            npcNameText == null ||
            dialogueText == null)
        {
            Debug.LogWarning("Dialogue UI references are missing!");
            return;
        }

        if (npc.dialogueLines == null ||
            npc.dialogueLines.Length == 0)
        {
            Debug.LogWarning("NPC has no dialogue lines!");
            return;
        }

        dialogueLines = npc.dialogueLines;
        currentLine = 0;
        dialogueActive = true;

        dialoguePanel.SetActive(true);

        // Display NPC name
        npcNameText.text = npc.npcName;

        // Display first dialogue line
        ShowLine();
    }

    // Continue to the next dialogue line
    public void NextLine()
    {
        if (!dialogueActive)
            return;

        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine();
        }
    }

    // Display current dialogue line
    private void ShowLine()
    {
        dialogueText.text = dialogueLines[currentLine];
    }

    // Close dialogue
    public void EndDialogue()
    {
        dialogueActive = false;
        dialoguePanel.SetActive(false);
    }
}


using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public string npcName = "Onkel";

    [TextArea(2, 5)]
    public string[] dialogueLines =
    {
        "Dine fjer er virkelig vokset det sidste år!",
        "Du er klar til at træde ind i familieforretningen.",
        "Men først skal du bevise, at du kan flyve!"
    };
}

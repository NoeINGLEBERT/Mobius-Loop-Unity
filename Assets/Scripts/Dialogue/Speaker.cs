using UnityEngine;
using System;

public class Speaker : MonoBehaviour
{
    [SerializeField]
    private Character character;

    public DialogueEntry dialogue;

    private void Start()
    {
        DialogueManager.Instance.speakers.Add(character, this);
    }

    public void DisplayDialogue(DialogueEntry Dialogue)
    {
        dialogue = Dialogue;

        DialogueManager.Instance.DisplayDialogue(dialogue.dialogueText, this);
    }
}

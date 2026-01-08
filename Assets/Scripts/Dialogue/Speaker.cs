using UnityEngine;
using System;

public class Speaker : MonoBehaviour
{
    [SerializeField]
    private Character character;

    public DialogueEntry dialogue;

    public string displayName;

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

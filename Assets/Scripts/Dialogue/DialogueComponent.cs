using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Reaction
{
    public int priority;

    public int newStartingIndex;

    public int newDialoguePoolSize;
}

[System.Serializable]
public class ReactionEntry
{
    public string events;           // "EventA, EventB"
    public Reaction[] reactions;
}

public class DialogueComponent : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private DialogueTable dialogueTable;
    [SerializeField] private int dialoguePoolSize = 1;
    [SerializeField] private int startingIndex = 0;

    [Header("State")]
    public int currentIndex;
    public int lastEventPriority;

    [Header("Reactions")]
    [SerializeField] private List<ReactionEntry> reactions = new();

    #region Dialogue Flow

    private void OnEnable()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnNotifyEvent += HandleEventNotify;
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnNotifyEvent -= HandleEventNotify;
    }

    public void HandleEventNotify(string Event)
    {
        foreach (ReactionEntry entry in reactions)
        {
            bool triggerReaction = false;

            string[] parsedEvents = entry.events.Split(',');

            triggerReaction = System.Array.Exists(parsedEvents, e => e.Trim() == Event);

            foreach (string parsedEvent in parsedEvents)
            {
                string trimmedEvent = parsedEvent.Trim();

                bool isIncomingEvent = trimmedEvent == Event;
                bool existsInGlobalEvents = System.Array.Exists(
                    DialogueManager.Instance.Events,
                    e => e == trimmedEvent
                );

                if (!isIncomingEvent && !existsInGlobalEvents)
                {
                    triggerReaction = false;
                    break;
                }
            }

            if (triggerReaction)
            {
                foreach (Reaction reaction in entry.reactions)
                {
                    if (reaction.priority > lastEventPriority)
                    {
                        startingIndex = reaction.newStartingIndex;
                        dialoguePoolSize = reaction.newDialoguePoolSize;
                        lastEventPriority = reaction.priority;
                    }
                }
            }
        }
    }

    // Retrieves a dialogue entry from an index
    public DialogueEntry GetDialogueFromIndex(int Index)
    {

        if (dialogueTable == null)
            return null;

        if (Index < 0 || Index >= dialogueTable.dialogues.Length)
            return null;

        return dialogueTable.dialogues[Index];
    }

    // Initializes dialogue state
    public void StartDialogue()
    {
        TryDialogue(startingIndex);
    }

    public void TryDialogue(int index)
    {
        currentIndex = index;

        if (GetDialogueFromIndex(currentIndex).dialogueText == "") return;

        PlayDialogue();
    }

    // Plays the current dialogue
    public void PlayDialogue(bool isEnd = false)
    {
        if (isEnd) return;

        DialogueEntry currentDialogue = GetDialogueFromIndex(currentIndex);

        DialogueManager.Instance.currentDialogueComponent = this;

        Speaker currentSpeaker = DialogueManager.Instance.speakers[currentDialogue.speaker];

        DialogueManager.Instance.NotifyEvent(currentDialogue.eventName);

        currentSpeaker.DisplayDialogue(currentDialogue);

        DialogueManager.Instance.OnDialogueSkipped -= PlayDialogue;

        if (currentDialogue.dialogueEnd)
        {
            if (dialoguePoolSize > 1)
            {
                startingIndex = currentIndex + 1;

                dialoguePoolSize--;
            }
        }
        else
        {
            if (currentDialogue.choicesTexts.Length != 0)
            {
                
            }
            else
            {
                DialogueManager.Instance.OnDialogueSkipped += PlayDialogue;

                currentIndex++;
            }
        }

    }

    #endregion
}

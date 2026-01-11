using System.Collections.Generic;
using UnityEngine;
using System.Collections;

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

    [Header("Dialogue Effects")]
    [SerializeField] private MonoBehaviour effectBehaviour;

    private ICellEffect dialogueEffect;

    private Coroutine bindCoroutine;

    private void Awake()
    {
        if (effectBehaviour != null)
            dialogueEffect = effectBehaviour as ICellEffect;
    }

    #region Dialogue Flow

    private void OnEnable()
    {
        bindCoroutine = StartCoroutine(WaitAndBind());
    }

    private void OnDisable()
    {
        if (bindCoroutine != null)
        {
            StopCoroutine(bindCoroutine);
            bindCoroutine = null;
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnNotifyEvent -= HandleEventNotify;
            Debug.Log($"[DialogueComponent] Unbound from DialogueManager on {gameObject.name}");
        }
    }

    private IEnumerator WaitAndBind()
    {
        Debug.Log($"[DialogueComponent] Waiting for DialogueManager on {gameObject.name}");

        yield return new WaitUntil(() =>
            DialogueManager.Instance != null &&
            DialogueManager.Instance.Events != null &&
            DialogueManager.Instance.speakers != null
        );

        DialogueManager.Instance.OnNotifyEvent += HandleEventNotify;

        Debug.Log($"[DialogueComponent] Successfully bound to DialogueManager on {gameObject.name}");
    }

    public void HandleEventNotify(string Event)
    {
        Debug.Log(Event);

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

        // =========================
        // EFFECT DIALOGUE
        // =========================
        if (currentDialogue.dialogueText == "[EFFECT]")
        {
            if (dialogueEffect != null)
            {
                StartCoroutine(PlayEffectAndResume());
                return;
            }
            else
            {
                Debug.LogWarning("[Dialogue] EFFECT tag found but no effect assigned.");
            }
        }

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

    private IEnumerator PlayEffectAndResume()
    {
        // Block dialogue skip input while effect runs
        DialogueManager.Instance.OnDialogueSkipped -= PlayDialogue;

        Pawn pawn = FindFirstObjectByType<PlayerActor>().GetComponent<Pawn>(); // or however you resolve player

        yield return dialogueEffect.Activate(pawn);

        // Advance dialogue index AFTER effect
        currentIndex++;

        // Resume dialogue normally
        PlayDialogue();
    }

    #endregion
}

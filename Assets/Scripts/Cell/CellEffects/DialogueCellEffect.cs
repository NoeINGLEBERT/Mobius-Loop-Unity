using System.Collections;
using UnityEngine;

[System.Serializable]
public class DialogueCellEffect : MonoBehaviour, ICellEffect
{
    [Header("Dialogue")]
    [SerializeField] private DialogueComponent dialogueComponent;

    public IEnumerator Activate(Pawn pawn)
    {
        if (!pawn.GetComponent<PlayerActor>()) yield break;

        if (dialogueComponent == null)
        {
            Debug.LogError("[DialogueCellEffect] No DialogueComponent assigned on this cell.");
            yield break;
        }

        bool dialogueFinished = false;

        // Subscribe to OnDialogueSkipped to detect when the dialogue ends
        void OnDialogueEnded(bool isEnd)
        {
            if (isEnd)
                dialogueFinished = true;
        }

        DialogueManager.Instance.OnDialogueSkipped += OnDialogueEnded;

        // Start the dialogue
        dialogueComponent.StartDialogue();

        // Wait until dialogue is finished
        while (!dialogueFinished)
            yield return null;

        // Cleanup subscription to prevent memory leaks
        DialogueManager.Instance.OnDialogueSkipped -= OnDialogueEnded;
    }
}

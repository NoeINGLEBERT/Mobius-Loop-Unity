using UnityEngine;

// Enum for all characters
public enum Character
{
    None,
    Alice,
    WhiteRabbit,
    CheshireCat,
    Bottle,
    Cake,
    Door
}

[System.Serializable]
public class DialogueEntry
{
    public Character speaker;          // Which character is speaking
    [TextArea(3, 10)]
    public string dialogueText;        // The main dialogue text

    public string[] choicesTexts;      // Options player can choose
    public int[] answersIndexes;       // Index of next dialogue or response

    public bool dialogueEnd;           // Is this the end of the dialogue?
    public string eventName;           // Event triggered by this dialogue
}

[CreateAssetMenu(fileName = "DialogueTable", menuName = "Scriptable Objects/DialogueTable")]
public class DialogueTable : ScriptableObject
{
    [Header("Dialogue Entries")]
    public DialogueEntry[] dialogues;      // Array of all dialogue entries in this NPC
}

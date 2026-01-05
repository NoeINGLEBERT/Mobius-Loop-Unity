using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Speakers")]
    public Dictionary<Character, Speaker> speakers = new Dictionary<Character, Speaker>();

    public event Action<string> OnNotifyEvent;
    public string[] Events;

    public Speaker currentSpeaker;
    public DialogueComponent currentDialogueComponent;

    private string currentDialogue;
    private int displayedLetters;

    public event Action<Speaker> OnDialogueDisplayed;

    private Coroutine typingCoroutine;
    [SerializeField] private float letterDelay = 0.02f;
    [SerializeField] private float fadeDuration = 0.3f;

    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private TMP_Text dialogueText;

    public event Action<bool> OnDialogueSkipped;

    [Header("Choice & Skip UI")]
    [SerializeField] private GameObject skipButton;
    [SerializeField] private CanvasGroup skipButtonCanvasGroup;
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private CanvasGroup choicePanelCanvasGroup;
    [SerializeField] private GameObject choicePrefab;
    [SerializeField] private Transform choiceContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void NotifyEvent(string Event)
    {
        OnNotifyEvent?.Invoke(Event);
    }

    public void DisplayDialogue(string DialogueText, Speaker speaker)
    {
        // Stop previous typing if any
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentDialogue = DialogueText;
        currentSpeaker = speaker;
        displayedLetters = 0;
        UpdateText(0);

        // Disable skip button and choice panel immediately
        skipButtonCanvasGroup.alpha = 0f;
        skipButtonCanvasGroup.interactable = false;
        skipButtonCanvasGroup.blocksRaycasts = false;

        choicePanelCanvasGroup.alpha = 0f;
        choicePanelCanvasGroup.interactable = false;
        choicePanelCanvasGroup.blocksRaycasts = false;

        // Clear previous choices
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);

        // Ensure dialogue panel is interactable and visible
        dialogueCanvasGroup.blocksRaycasts = true;
        dialogueCanvasGroup.interactable = true;

        // Fade in the dialogue panel first
        StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f, fadeDuration, () =>
        {
            // Start typing after panel fade-in
            typingCoroutine = StartCoroutine(TypeDialogue());
        }));
    }

    private IEnumerator TypeDialogue()
    {
        while (displayedLetters <= currentDialogue.Length)
        {
            yield return new WaitForSeconds(letterDelay);
            displayedLetters++;
            UpdateText(displayedLetters);
        }

        // Dialogue fully displayed, now fade in skip button or choice panel
        bool hasChoices = currentSpeaker.dialogue.choicesTexts != null &&
                          currentSpeaker.dialogue.choicesTexts.Length > 0;

        if (hasChoices)
        {
            GenerateChoices(currentSpeaker.dialogue);
            StartCoroutine(FadeCanvasGroup(choicePanelCanvasGroup, 0f, 1f, fadeDuration));
        }
        else
        {
            // Fade in skip button
            StartCoroutine(FadeCanvasGroup(skipButtonCanvasGroup, 0f, 1f, fadeDuration, () =>
            {
                // Enable interaction only after fade-in
                skipButtonCanvasGroup.interactable = true;
                skipButtonCanvasGroup.blocksRaycasts = true;
            }));
        }
    }

    private void UpdateText(int DisplayedLetters)
    {
        displayedLetters = DisplayedLetters;

        if (displayedLetters <= currentDialogue.Length)
        {
            dialogueText.text = currentDialogue.Substring(0, displayedLetters);
        }
        else
        {
            // Ensure full text is shown
            dialogueText.text = currentDialogue;

            OnDialogueDisplayed?.Invoke(currentSpeaker);
            OnDialogueDisplayed = null;
        }
    }

    private void GenerateChoices(DialogueEntry dialogue)
    {
        // Clear previous choices
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);

        // Generate new choices
        for (int i = 0; i < dialogue.choicesTexts.Length; i++)
        {
            GameObject choiceGO = Instantiate(choicePrefab, choiceContainer);
            TMP_Text choiceText = choiceGO.GetComponentInChildren<TMP_Text>();
            if (choiceText != null) choiceText.text = dialogue.choicesTexts[i];

            int index = i;
            Button button = choiceGO.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() =>
                {
                    currentDialogueComponent.TryDialogue(dialogue.answersIndexes[index]);
                });
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, Action onComplete = null)
    {
        float t = 0f;
        cg.alpha = from;
        cg.blocksRaycasts = to > 0f;
        cg.interactable = to > 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        cg.alpha = to;
        onComplete?.Invoke();
    }

    public void SkipDialogue()
    {
        if (currentSpeaker == null) return;

        bool isEnd = currentSpeaker.dialogue.dialogueEnd;

        // Fade out dialogue panel if dialogue ends
        if (isEnd)
        {
            StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, dialogueCanvasGroup.alpha, 0f, fadeDuration));
        }

        currentSpeaker = null;

        OnDialogueSkipped?.Invoke(isEnd);
    }
}

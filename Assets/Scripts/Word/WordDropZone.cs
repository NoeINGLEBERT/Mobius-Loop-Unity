using UnityEngine;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class WordDropZone : MonoBehaviour, IDropHandler
{
    public event Action OnZoneChanged;

    private WordButton currentWord;

    public bool HasWord() => currentWord != null;
    public WordButton GetWord() => currentWord;

    public void SetWord(WordButton word)
    {
        // If another word is already here, send it back
        if (currentWord != null)
        {
            currentWord.GetComponent<WordDragHandler>().ReturnToOriginal();
            ClearWord();
        }

        // If word was in another zone, clear it
        WordDropZone previousZone = word.GetComponentInParent<WordDropZone>();
        if (previousZone != null)
        {
            previousZone.ClearWord();
        }

        // Parent the word to this zone
        word.transform.SetParent(transform);

        RectTransform rect = word.GetComponent<RectTransform>();

        // Force center anchoring
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);

        // Snap perfectly to center
        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        // Notify drag handler
        word.GetComponent<WordDragHandler>().MarkDroppedInTarget();

        currentWord = word;

        GetComponentInChildren<TMP_Text>().text = "";

        OnZoneChanged?.Invoke();
    }

    public void ClearWord()
    {
        if (currentWord == null)
            return;

        currentWord = null;
        OnZoneChanged?.Invoke();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        WordButton droppedWord = eventData.pointerDrag.GetComponent<WordButton>();
        if (droppedWord == null)
            return;

        SetWord(droppedWord);
    }

    public void UnbindAll()
    {
        // Prevent re-trigger
        OnZoneChanged = null;
    }
}

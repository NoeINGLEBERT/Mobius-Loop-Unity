using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class DropZone : MonoBehaviour, IDropHandler
{
    public event Action OnZoneChanged;

    private Card currentCard;

    public bool HasCard() => currentCard != null;
    public Card GetCard() => currentCard;

    public void SetCard(Card card)
    {
        // If another card is already here, send it back to hand
        if (currentCard != null)
        {
            currentCard.ReturnToParent(card);
            ClearCard();
        }

        // If card was in another drop zone, free it
        DropZone previousZone = card.GetComponentInParent<DropZone>();
        if (previousZone != null)
        {
            previousZone.ClearCard();
        }

        // Parent the card to this zone
        card.transform.SetParent(transform);

        RectTransform cardRect = card.GetComponent<RectTransform>();

        // Force center anchoring
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);

        // Snap perfectly to center
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localRotation = Quaternion.identity;
        cardRect.localScale = Vector3.one;

        card.SetDroppedInZone(true);

        currentCard = card;

        card.OnCardChanged += OnZoneChanged;

        OnZoneChanged?.Invoke();
    }

    public void ClearCard()
    {
        if (currentCard == null)
            return;

        currentCard.OnCardChanged -= OnZoneChanged;
        currentCard = null;

        OnZoneChanged?.Invoke();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        Card droppedCard = eventData.pointerDrag.GetComponent<Card>();
        if (droppedCard == null)
            return;

        SetCard(droppedCard);
    }
}


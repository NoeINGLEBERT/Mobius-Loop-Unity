using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Card))]
[RequireComponent(typeof(CanvasGroup))]
public class CardDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private Transform originalParent;
    private Vector2 originalPosition;

    private Transform handTransform;
    private Vector2 handPosition;

    private bool droppedInZone = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        // Store hand transform
        handTransform = transform.parent;
        handPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Remember where it came from
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        // If coming from a drop zone, clear it
        DropZone zone = GetComponentInParent<DropZone>();
        if (zone != null)
        {
            zone.ClearCard();
        }

        droppedInZone = false;

        // Allow raycasts to go through this card (so drop zones can detect it)
        canvasGroup.blocksRaycasts = false;

        // Optional: bring the card visually to the front
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!droppedInZone)
        {
            ReturnToHand();
        }
    }

    public void MarkDroppedInZone()
    {
        droppedInZone = true;
    }

    public void ReturnToParent(CardDragHandler card)
    {
        // Snap back to card parent
        transform.SetParent(card.originalParent);
        rectTransform.anchoredPosition = card.originalPosition;

        DropZone zone = card.originalParent.GetComponent<DropZone>();
        if (zone != null)
        {
            zone.SetCard(this.GetComponent<Card>());
        }
    }

    public void ReturnToHand()
    {
        // Snap back to hand
        transform.SetParent(handTransform);
        rectTransform.anchoredPosition = handPosition;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData cardData;

    public event Action OnCardChanged;
    public static event Action OnFaceSwap;

    [SerializeField] GameObject symbolPrefab;
    [SerializeField] Transform frontPanel;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform handTransform;
    private Vector2 handPosition;

    private Transform originalParent;
    private Vector2 originalPosition;

    private bool droppedInZone = false;

    public event Action<Card> OnDiscardRequested;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        LoadFace();

        // Store hand transform
        handTransform = transform.parent;
        handPosition = rectTransform.anchoredPosition;
    }

    void LoadFace()
    {
        foreach (Transform child in frontPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (string letter in cardData._isFront ? cardData._frontFace : cardData._backFace)
        {
            GameObject newSymbol = Instantiate(symbolPrefab, frontPanel);
            newSymbol.GetComponent<Symbol>().SetSymbol(letter, cardData._isFront ? cardData._frontSuit : cardData._backSuit);
        }
    }

    public void SwapFace()
    {
        cardData._isFront = !cardData._isFront;
        LoadFace();

        OnCardChanged?.Invoke();
        OnFaceSwap?.Invoke();
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


    public void ReturnToParent(Card card)
    {
        // Snap back to card parent
        transform.SetParent(card.originalParent);
        rectTransform.anchoredPosition = card.originalPosition;

        DropZone zone = card.originalParent.GetComponent<DropZone>();
        if (zone != null)
        {
            zone.SetCard(this);
        }
    }

    public void ReturnToHand()
    {
        // Snap back to hand
        transform.SetParent(handTransform);
        rectTransform.anchoredPosition = handPosition;
    }

    public void SetDroppedInZone(bool value)
    {
        droppedInZone = value;
    }

    public void Discard()
    {
        // If in a drop zone, clear it properly
        DropZone zone = GetComponentInParent<DropZone>();
        if (zone != null)
        {
            zone.ClearCard();
        }

        OnDiscardRequested?.Invoke(this);
    }
}

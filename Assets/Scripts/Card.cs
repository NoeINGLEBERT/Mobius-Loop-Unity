using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData cardData;

    [SerializeField] GameObject symbolPrefab;
    [SerializeField] Transform frontPanel;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private bool droppedInZone = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        LoadFace();
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
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Remember where it came from
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

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
            // Snap back to hand
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }

        droppedInZone = false; // reset for next drag
    }

    public void SetDroppedInZone(bool value)
    {
        droppedInZone = value;
    }
}

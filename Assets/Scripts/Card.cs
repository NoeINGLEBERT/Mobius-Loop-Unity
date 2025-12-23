using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using TMPro;


public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData cardData;

    public event Action OnCardChanged;
    public static event Action OnFaceSwap;

    [SerializeField] GameObject symbolPrefab;
    [SerializeField] Transform frontPanel;
    [SerializeField] private TMP_Text backLettersText;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform handTransform;
    private Vector2 handPosition;

    private Transform originalParent;
    private Vector2 originalPosition;

    private bool droppedInZone = false;

    public event Action<Card> OnDiscardRequested;
    public event Action<Card> OnDestroyRequested;

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
        // Clear current face visuals
        foreach (Transform child in frontPanel)
        {
            Destroy(child.gameObject);
        }

        // Select which face to render
        var faceLetters = cardData._isFront
            ? cardData._frontFace
            : cardData._backFace;

        var backLetters = cardData._isFront
            ? cardData._backFace
            : cardData._frontFace;

        var suit = cardData._isFront
            ? cardData._frontSuit
            : cardData._backSuit;

        // Render symbols
        foreach (string letter in faceLetters)
        {
            GameObject newSymbol = Instantiate(symbolPrefab, frontPanel);
            newSymbol.GetComponent<Symbol>().SetSymbol(letter, suit);
        }

        // Write back-face letters as comma-separated text
        if (backLettersText != null)
        {
            backLettersText.text = string.Join(", ", backLetters);
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
        StartCoroutine(DiscardAnim());
    }

    public void Destroy()
    {
        StartCoroutine(DestroyAnim());
    }


    // ANIMATIONS

    public void PlayDrawAnimation(float duration = 0.25f)
    {
        StartCoroutine(DrawAnim(duration));
    }

    private IEnumerator DrawAnim(float duration)
    {
        float t = 0f;

        Vector3 startScale = Vector3.one * 0.8f;
        Vector3 endScale = Vector3.one;

        canvasGroup.alpha = 0f;
        rectTransform.localScale = startScale;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        rectTransform.localScale = endScale;
        canvasGroup.alpha = 1f;
    }

    private IEnumerator DiscardAnim()
    {
        float duration = 0.25f;
        float t = 0f;

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * 80f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            rectTransform.anchoredPosition =
                Vector2.Lerp(startPos, endPos, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        OnDiscardRequested?.Invoke(this);
    }

    private IEnumerator DestroyAnim()
    {
        float shakeDuration = 0.15f;
        float shrinkDuration = 0.15f;

        Vector2 originalPos = rectTransform.anchoredPosition;

        // Shake
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / shakeDuration;

            rectTransform.anchoredPosition =
                originalPos + UnityEngine.Random.insideUnitCircle * 8f;

            yield return null;
        }

        rectTransform.anchoredPosition = originalPos;

        // Shrink
        t = 0f;
        Vector3 startScale = rectTransform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime / shrinkDuration;
            rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        OnDestroyRequested?.Invoke(this);
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;

[RequireComponent(typeof(Card))]
public class CardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float hoverSpeed = 10f;

    private Card card;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 targetScale;

    // Event for external scripts to subscribe
    public event Action<Card> OnCardClicked;

    void Awake()
    {
        card = GetComponent<Card>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Smoothly scale toward target
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * hoverSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Fire event so other scripts can handle the card being clicked
        OnCardClicked?.Invoke(card);
    }
}

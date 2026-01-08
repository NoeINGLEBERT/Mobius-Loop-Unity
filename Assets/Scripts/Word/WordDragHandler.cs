using UnityEngine;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(WordButton))]
public class WordDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private Vector2 originalPosition;
    private Transform originalParent;

    private bool droppedInTarget = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        droppedInTarget = false;

        // Allow raycasts to go through so drop targets can detect it
        canvasGroup.blocksRaycasts = false;

        // Optional: move to top layer
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!droppedInTarget)
            ReturnToOriginal();
    }

    public void MarkDroppedInTarget()
    {
        droppedInTarget = true;
    }

    public void ReturnToOriginal()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}

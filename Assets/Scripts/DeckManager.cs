using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DeckManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handArea;

    [Header("Deck Setup")]
    [SerializeField] private int handSize = 5;
    [SerializeField] private Deck deck;

    [Header("Draw Settings")]
    [SerializeField] private float refillDelay = 0.25f;   // retriggerable delay
    [SerializeField] private float drawInterval = 0.1f;   // time between each card

    private readonly List<CardData> drawPile = new();
    private readonly List<CardData> discardPile = new();
    private readonly List<Card> handCards = new();

    public int HandCount => handCards.Count;

    void Start()
    {
        GetComponent<DeckGenerator>().GenerateDeck();

        drawPile.AddRange(deck.cards);
        Shuffle(drawPile);

        RefillHand();
    }

    // =========================
    // DRAW / DISCARD
    // =========================

    public void RefillHand()
    {
        StartCoroutine(RefillHandDelayed());
    }

    private IEnumerator RefillHandDelayed()
    {
        // Draw missing cards one by one
        while (handCards.Count < handSize)
        {
            if (!DrawOne())
                break;

            yield return new WaitForSeconds(drawInterval);
        }
    }

    private bool DrawOne()
    {
        if (drawPile.Count == 0)
            RefillFromDiscard();

        if (drawPile.Count == 0)
        {
            GameOver();
            return false;
        }

        CardData data = drawPile[0];
        drawPile.RemoveAt(0);

        GameObject go = Instantiate(cardPrefab, handArea);
        go.AddComponent<CardDragHandler>();
        Card card = go.GetComponent<Card>();
        card.cardData = data;

        handCards.Add(card);
        card.PlayDrawAnimation();

        return true;
    }

    public void HandleDiscard(Card card)
    {
        card.OnDiscardRequested -= HandleDiscard;

        handCards.Remove(card);
        discardPile.Add(card.cardData);

        Destroy(card.gameObject);
    }

    public void HandleDestroy(Card card)
    {
        card.OnDestroyRequested -= HandleDestroy;

        handCards.Remove(card);
    }

    // =========================
    // RESHUFFLE
    // =========================

    private void RefillFromDiscard()
    {
        if (discardPile.Count == 0)
            return;

        drawPile.AddRange(discardPile);
        discardPile.Clear();

        Shuffle(drawPile);
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER");
    }
}

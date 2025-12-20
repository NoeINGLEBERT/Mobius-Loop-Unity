using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handArea;

    [Header("Deck Setup")]
    [SerializeField] private int handSize = 5;
    [SerializeField] private Deck deck;

    private readonly List<CardData> drawPile = new();
    private readonly List<CardData> discardPile = new();
    private readonly List<Card> handCards = new();

    void Start()
    {
        GetComponent<DeckGenerator>().GenerateDeck();

        drawPile.AddRange(deck.cards);
        Shuffle(drawPile);

        DrawUpToHandSize();
    }

    // =========================
    // DRAW / DISCARD
    // =========================

    private void DrawUpToHandSize()
    {
        while (handCards.Count < handSize)
        {
            DrawOne();
        }
    }

    private void DrawOne()
    {
        if (drawPile.Count == 0)
            RefillFromDiscard();

        if (drawPile.Count == 0)
            return; // no cards left at all

        CardData data = drawPile[0];
        drawPile.RemoveAt(0);

        GameObject go = Instantiate(cardPrefab, handArea);
        Card card = go.GetComponent<Card>();
        card.cardData = data;

        card.OnDiscardRequested += HandleDiscard;

        handCards.Add(card);
    }

    private void HandleDiscard(Card card)
    {
        card.OnDiscardRequested -= HandleDiscard;

        handCards.Remove(card);
        discardPile.Add(card.cardData);

        Destroy(card.gameObject);

        DrawUpToHandSize();
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
}

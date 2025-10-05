using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [Header("References")]
    public GameObject cardPrefab;
    public Transform handArea;

    [Header("Deck Setup")]
    public int startingHandSize = 5;

    [SerializeField] private Deck deck;

    void Start()
    {
        GetComponent<DeckGenerator>().GenerateDeck();

        // Draw starting hand
        DrawCards(startingHandSize);
    }

    void DrawCards(int count)
    {
        for (int i = 0; i < count && deck.cards.Length > 0; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, handArea);
            newCard.GetComponent<Card>().cardData = deck.cards[i];
        }
    }
}

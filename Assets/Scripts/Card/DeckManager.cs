using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform handArea;

    [Header("Deck Setup")]
    [SerializeField] private int handSize = 5;
    [SerializeField] private Deck deck;

    [Header("Draw Settings")]
    [SerializeField] private float refillDelay = 0.25f;   // retriggerable delay
    [SerializeField] private float drawInterval = 0.1f;   // time between each card

    [Header("UI")]
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private TMP_Text discardCountText;
    [SerializeField] private TMP_Text sanityText;

    private readonly List<CardData> drawPile = new();
    private readonly List<CardData> discardPile = new();
    private readonly List<Card> handCards = new();

    public int HandCount => handCards.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        GetComponent<DeckGenerator>().GenerateDeck();

        drawPile.AddRange(deck.cards);
        Shuffle(drawPile);

        UpdateUI();
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

        UpdateUI();
        return true;
    }

    public void HandleDiscard(Card card)
    {
        card.OnDiscardRequested -= HandleDiscard;

        handCards.Remove(card);
        discardPile.Add(card.cardData);

        Destroy(card.gameObject);
        UpdateUI();
    }

    public void HandleDestroy(Card card)
    {
        card.OnDestroyRequested -= HandleDestroy;

        handCards.Remove(card);
        UpdateUI();
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
        UpdateUI();
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void UpdateUI()
    {
        int deckCount = drawPile.Count;
        int discardCount = discardPile.Count;
        int sanity = deckCount + discardCount;

        if (deckCountText)
            deckCountText.text = deckCount.ToString();

        if (discardCountText)
            discardCountText.text = discardCount.ToString();

        if (sanityText)
        {
            sanityText.text = sanity.ToString() + " cards left";

            sanityText.color = sanity > 10 ? Color.white : Color.red;
        }
    }

    private void GameOver()
    {
        GameStateManager.Instance.TriggerDefeat();
    }
}

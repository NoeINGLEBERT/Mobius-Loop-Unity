using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DeckGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int deckSize = 20;
    public int cardTargetPoints = 10;
    public int maxLettersPerSide = 4;

    [Header("Output")]
    public Deck generatedDeck;

    private List<LetterData> scrabbleLetters;
    private List<char> weightedLetterPool;
    private Dictionary<char, int> letterPoints;

    void Awake()
    {
        InitScrabbleLetters();
    }

    [ContextMenu("Generate Deck")]
    public void GenerateDeck()
    {
        if (generatedDeck == null)
        {
            Debug.LogError("No Deck ScriptableObject assigned!");
            return;
        }

        List<CardData> cards = new List<CardData>();

        for (int i = 0; i < deckSize; i++)
        {
            CardData card = GenerateCard(cardTargetPoints);
            cards.Add(card);
        }

        generatedDeck.cards = cards.ToArray();

#if UNITY_EDITOR
        EditorUtility.SetDirty(generatedDeck);
#endif

        Debug.Log($"Generated deck with {generatedDeck.cards.Length} cards.");
    }

    private CardData GenerateCard(int targetPoints)
    {
        List<string> front = new List<string>();
        List<string> back = new List<string>();

        int frontPoints = 0;
        int backPoints = 0;

        int safety = 2000; // prevent infinite loops

        while (safety-- > 0)
        {
            int totalPoints = frontPoints + backPoints;

            // Stop if total = 10 and both have at least 1 letter
            if (totalPoints == targetPoints && front.Count > 0 && back.Count > 0)
                break;

            // Pick a random side to add a letter to
            bool addToFront = Random.value < 0.5f;

            var side = addToFront ? front : back;
            var sidePoints = addToFront ? frontPoints : backPoints;

            if (side.Count >= maxLettersPerSide)
                continue;

            // Get all possible letters that don't exceed 10 and aren't already used on this face
            List<char> validLetters = weightedLetterPool
                .Distinct()
                .Where(c =>
                {
                    if (c == '?') return false; // skip blanks for now

        // A card cannot have the same letter twice — on either face
        if (front.Contains(c.ToString()) || back.Contains(c.ToString()))
            return false;

        int pts = letterPoints[c];
        return totalPoints + pts <= targetPoints && sidePoints + pts <= targetPoints;
                })
                .ToList();

            // If no valid letters available
            if (validLetters.Count == 0)
            {
                // Only add a blank if this face has no letters at all
                if (side.Count == 0)
                {
                    side.Add("?");
                    // no points added since blanks = 0
                }
                // If both faces are impossible to continue, break to avoid infinite loop
                if (frontPoints + backPoints < targetPoints)
                {
                    // Try switching side once
                    if (addToFront && back.Count == 0)
                    {
                        addToFront = false;
                        continue;
                    }
                    else if (!addToFront && front.Count == 0)
                    {
                        addToFront = true;
                        continue;
                    }
                }
                break;
            }

            // Pick a valid letter weighted by frequency (but not duplicates)
            char chosen = validLetters[Random.Range(0, validLetters.Count)];
            int points = letterPoints[chosen];

            side.Add(chosen.ToString());
            if (addToFront)
                frontPoints += points;
            else
                backPoints += points;
        }

        // Safety fallback: ensure both sides have something
        if (front.Count == 0) front.Add("?");
        if (back.Count == 0) back.Add("?");

        // --- Random Suit Assignment ---
        Suit[] redSuits = { Suit.Heart, Suit.Diamond };
        Suit[] blackSuits = { Suit.Club, Suit.Spade };

        bool frontIsRed = Random.value < 0.5f;

        Suit frontSuit = frontIsRed
            ? redSuits[Random.Range(0, redSuits.Length)]
            : blackSuits[Random.Range(0, blackSuits.Length)];

        Suit backSuit = frontIsRed
            ? blackSuits[Random.Range(0, blackSuits.Length)]
            : redSuits[Random.Range(0, redSuits.Length)];

        bool isFront = Random.value < 0.5f;

        int finalTotal = frontPoints + backPoints;
        if (finalTotal != targetPoints)
        {
            Debug.LogError(
                $"[DeckGenerator] Card generation failed to hit target points! " +
                $"Got {finalTotal} instead of {targetPoints}.\n" +
                $"Front: [{string.Join(",", front)}] ({frontPoints} pts) | " +
                $"Back: [{string.Join(",", back)}] ({backPoints} pts)"
            );
        }

        return new CardData(front.ToArray(), back.ToArray(), isFront, frontSuit, backSuit);
    }

    private void InitScrabbleLetters()
    {
        scrabbleLetters = new List<LetterData>()
        {
            new LetterData('?', 0, 2),
            new LetterData('E', 1, 12), new LetterData('A', 1, 9), new LetterData('I', 1, 9),
            new LetterData('O', 1, 8), new LetterData('N', 1, 6), new LetterData('R', 1, 6),
            new LetterData('T', 1, 6), new LetterData('L', 1, 4), new LetterData('S', 1, 4),
            new LetterData('U', 1, 4), new LetterData('D', 2, 4), new LetterData('G', 2, 3),
            new LetterData('B', 3, 2), new LetterData('C', 3, 2), new LetterData('M', 3, 2),
            new LetterData('P', 3, 2), new LetterData('F', 4, 2), new LetterData('H', 4, 2),
            new LetterData('V', 4, 2), new LetterData('W', 4, 2), new LetterData('Y', 4, 2),
            new LetterData('K', 5, 1), new LetterData('J', 8, 1), new LetterData('X', 8, 1),
            new LetterData('Q', 10, 1), new LetterData('Z', 10, 1),
        };

        // Build weighted pool and point dictionary
        weightedLetterPool = new List<char>();
        letterPoints = new Dictionary<char, int>();

        foreach (var l in scrabbleLetters)
        {
            for (int i = 0; i < l.count; i++)
                weightedLetterPool.Add(l.letter);
            letterPoints[l.letter] = l.points;
        }
    }

    [System.Serializable]
    public struct LetterData
    {
        public char letter;
        public int points;
        public int count;

        public LetterData(char letter, int points, int count)
        {
            this.letter = letter;
            this.points = points;
            this.count = count;
        }
    }
}

using System.Collections.Generic;

public static class LetterRules
{
    // Scrabble-like base scores
    private static readonly Dictionary<char, int> LetterScores = new()
    {
        ['A'] = 1,
        ['E'] = 1,
        ['I'] = 1,
        ['O'] = 1,
        ['N'] = 1,
        ['R'] = 1,
        ['T'] = 1,
        ['L'] = 1,
        ['S'] = 1,
        ['U'] = 1,

        ['D'] = 2,
        ['G'] = 2,

        ['B'] = 3,
        ['C'] = 3,
        ['M'] = 3,
        ['P'] = 3,

        ['F'] = 4,
        ['H'] = 4,
        ['V'] = 4,
        ['W'] = 4,
        ['Y'] = 4,

        ['K'] = 5,

        ['J'] = 8,
        ['X'] = 8,

        ['Q'] = 10,
        ['Z'] = 10,

        ['-'] = 0 // blank
    };

    public static int GetLetterScore(char c)
    {
        c = char.ToUpperInvariant(c);

        return LetterScores.TryGetValue(c, out int score)
            ? score
            : 0;
    }

    public static int GetLetterScore(string letter)
    {
        if (string.IsNullOrEmpty(letter))
            return 0;

        return GetLetterScore(letter[0]);
    }

    public static int ComputeWordScore(string word)
    {
        int score = 0;

        foreach (char c in word)
        {
            score += GetLetterScore(c);
        }

        return score;
    }
}
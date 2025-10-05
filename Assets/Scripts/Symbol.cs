using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Suit
{
    None,
    Heart,
    Diamond,
    Club,
    Spade
}

public class Symbol : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image suitImage;
    [SerializeField] private TMP_Text letterText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Suit Sprites")]
    [SerializeField] private Sprite heartSprite;
    [SerializeField] private Sprite diamondSprite;
    [SerializeField] private Sprite clubSprite;
    [SerializeField] private Sprite spadeSprite;

    private Suit currentSuit = Suit.None;
    private string letter;

    public void SetSymbol(string letterChar, Suit suit)
    {
        letterText.text = letterChar;
        scoreText.text = GetScrabbleScore(letterChar).ToString();
        SetSuit(suit);
    }

    private void SetSuit(Suit suit)
    {
        currentSuit = suit;

        if (suitImage == null) return;

        switch (suit)
        {
            case Suit.Heart:
                suitImage.sprite = heartSprite;
                scoreText.color = Color.red;
                break;

            case Suit.Diamond:
                suitImage.sprite = diamondSprite;
                scoreText.color = new Color(1f, 0.3f, 0.3f);
                break;

            case Suit.Club:
                suitImage.sprite = clubSprite;
                scoreText.color = Color.black;
                break;

            case Suit.Spade:
                suitImage.sprite = spadeSprite;
                scoreText.color = Color.black;
                break;

            default:
                suitImage.sprite = null;
                scoreText.color = Color.clear;
                break;
        }
    }

    private int GetScrabbleScore(string letter)
    {
        if (string.IsNullOrEmpty(letter))
            return 0;

        char c = char.ToUpperInvariant(letter[0]);
        switch (c)
        {
            case 'A': case 'E': case 'I': case 'O': case 'N': case 'R': case 'T': case 'L': case 'S': case 'U': return 1;
            case 'D': case 'G': return 2;
            case 'B': case 'C': case 'M': case 'P': return 3;
            case 'F': case 'H': case 'V': case 'W': case 'Y': return 4;
            case 'K': return 5;
            case 'J': case 'X': return 8;
            case 'Q': case 'Z': return 10;
            default: return 0;
        }
    }
}

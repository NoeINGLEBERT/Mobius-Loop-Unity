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
        scoreText.text = LetterRules.GetLetterScore(letterChar).ToString();
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
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("Pulse")]
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseSpeed = 6f;

    private Coroutine pulseRoutine;
    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    public string Letter => letterText.text;

    public void StartPulse()
    {
        if (pulseRoutine != null) return;
        pulseRoutine = StartCoroutine(Pulse());
    }

    public void StopPulse()
    {
        if (pulseRoutine == null) return;

        StopCoroutine(pulseRoutine);
        pulseRoutine = null;
        transform.localScale = baseScale;
    }

    private IEnumerator Pulse()
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * pulseSpeed;
            float s = 1f + Mathf.Sin(t) * (pulseScale - 1f);
            transform.localScale = baseScale * s;
            yield return null;
        }
    }

    public void SetSymbol(string letterChar, Suit suit)
    {
        letterText.text = letterChar;
        scoreText.text = LetterRules.GetLetterScore(letterChar).ToString();
        SetSuit(suit);
    }

    private void SetSuit(Suit suit)
    {
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

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardChoiceButton : MonoBehaviour
{
    [Header("References")]
    public Button button;                           // Assign in Inspector
    public CardChoiceCellEffect cardEffect;         // Assign the effect to activate
    public ScoreData scoreData;                     // Assign the ScriptableObject score

    public Pawn pawn;

    [Header("Cost Settings")]
    public int cost = 50;

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        if (scoreData == null || cardEffect == null) return;

        // Check if player has enough points
        if (scoreData.score >= cost)
        {
            scoreData.score -= cost;

            // Start the cell effect coroutine
            StartCoroutine(cardEffect.Activate(pawn)); // 'Pawn' can be passed if needed
        }
        else
        {
            Debug.Log("Not enough points to activate the card effect!");
        }
    }
}
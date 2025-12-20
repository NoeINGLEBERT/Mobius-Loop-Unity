using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct WordResult
{
    public string word;
    public int score;

    public WordResult(string word, int score)
    {
        this.word = word;
        this.score = score;
    }
}

public class WordButton : MonoBehaviour
{
    [SerializeField] TMP_Text wordText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] Button button;

    private WordResult result;
    private Pawn pawn;
    private WordValidator validator;

    public void Setup(WordResult result, Pawn pawn, WordValidator validator)
    {
        this.result = result;
        this.pawn = pawn;
        this.validator = validator;

        wordText.text = result.word;
        scoreText.text = result.score.ToString();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        validator.ConsumeCards();
        pawn.MoveUpCells(result.score, 1f);
    }
}

using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public ScoreData scoreData;           // assign in Inspector
    public TextMeshProUGUI scoreText;

    void Update()
    {
        if (scoreData != null)
            scoreText.text = scoreData.score.ToString();
    }
}
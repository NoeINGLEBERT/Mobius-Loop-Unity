using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public ScoreData scoreData; // assign in Inspector

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddPoint(int amount)
    {
        if (scoreData != null)
        {
            scoreData.AddPoints(amount);
        }
    }

    public int GetScore()
    {
        return scoreData != null ? scoreData.score : 0;
    }
}
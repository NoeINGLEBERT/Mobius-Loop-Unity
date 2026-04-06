using UnityEngine;

[CreateAssetMenu(fileName = "ScoreData", menuName = "Game/Score Data")]
public class ScoreData : ScriptableObject
{
    public int score = 0;

    // Optional: helper method to add points
    public void AddPoints(int amount)
    {
        score += amount;
        // Debug.Log("Score: " + score);
    }
}
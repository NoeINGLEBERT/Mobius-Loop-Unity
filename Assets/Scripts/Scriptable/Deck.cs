using UnityEngine;

[CreateAssetMenu(fileName = "Deck", menuName = "Scriptable Objects/Deck")]
public class Deck : ScriptableObject
{
    public CardData[] cards;
}

[System.Serializable]
public struct CardData
{
    public string[] _frontFace;
    public string[] _backFace;
    public bool _isFront;
    public Suit _frontSuit;
    public Suit _backSuit;

    // Constructor
    public CardData(string[] front, string[] back, bool isFront, Suit frontSuit, Suit backSuit)
    {
        _frontFace = front;
        _backFace = back;
        _isFront = isFront;
        _frontSuit = frontSuit;
        _backSuit = backSuit;
    }
}
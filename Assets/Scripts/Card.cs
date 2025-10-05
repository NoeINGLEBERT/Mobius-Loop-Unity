using UnityEngine;

public class Card : MonoBehaviour
{
    public CardData cardData;

    [SerializeField] GameObject symbolPrefab;
    [SerializeField] Transform frontPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (string letter in cardData._isFront ? cardData._frontFace : cardData._backFace)
        {
            GameObject newSymbol = Instantiate(symbolPrefab, frontPanel);
            newSymbol.GetComponent<Symbol>().SetSymbol(letter, cardData._isFront ? cardData._frontSuit : cardData._backSuit);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

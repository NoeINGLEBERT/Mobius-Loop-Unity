using UnityEngine;

public class GenerateDeck : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        GetComponent<DeckGenerator>().GenerateDeck();
    }


    // Update is called once per frame
    void Start()
    {

    }
}

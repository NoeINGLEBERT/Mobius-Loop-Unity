using System.Collections.Generic;
using UnityEngine;

public class WordValidator : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private int zoneCount = 5;
    [SerializeField] private DropZone zonePrefab;
    [SerializeField] private Transform zonesParent;
    [SerializeField] private string dictionaryFileName = "words.txt";

    [Header("UI")]
    [SerializeField] private Transform resultsPanel;
    [SerializeField] private WordButton wordButtonPrefab;

    [Header("Gameplay")]
    [SerializeField] private Pawn pawn;

    private WordDictionary dictionary;
    private DropZone[] zones;
    private readonly List<GameObject> spawnedButtons = new();

    private void Awake()
    {
        dictionary = new WordDictionary();

        string path = System.IO.Path.Combine(
            Application.streamingAssetsPath,
            dictionaryFileName
        );
        dictionary.LoadFromText(path);

        CreateZones();
    }

    private void OnEnable()
    {
        if (zones == null)
            return;

        foreach (DropZone zone in zones)
            zone.OnZoneChanged += Validate;
    }

    private void OnDisable()
    {
        if (zones == null)
            return;

        foreach (DropZone zone in zones)
            zone.OnZoneChanged -= Validate;
    }

    private void CreateZones()
    {
        zones = new DropZone[zoneCount];

        for (int i = 0; i < zoneCount; i++)
        {
            DropZone zone = Instantiate(zonePrefab, zonesParent);
            zones[i] = zone;
        }
    }

    /// Called automatically when any zone changes
    public void Validate()
    {
        ClearResults();

        if (!AllZonesFilled())
            return;

        List<WordResult> validWords = GetValidWords();

        foreach (WordResult result in validWords)
        {
            SpawnButton(result);
        }
    }

    private bool AllZonesFilled()
    {
        foreach (DropZone zone in zones)
        {
            if (!zone.HasCard())
                return false;
        }
        return true;
    }

    private List<WordResult> GetValidWords()
    {
        List<List<string>> letterPools = new();

        foreach (DropZone zone in zones)
        {
            Card card = zone.GetCard();

            List<string> letters = new(
                card.cardData._isFront
                    ? card.cardData._frontFace
                    : card.cardData._backFace
            );

            letterPools.Add(letters);
        }

        List<WordResult> results = new();
        GenerateCombinations(letterPools, 0, "", results);
        return results;
    }

    private void GenerateCombinations(List<List<string>> pools, int index, string current, List<WordResult> results)
    {
        if (index == pools.Count)
        {
            if (dictionary.IsValid(current))
            {
                results.Add(new WordResult(
                    current,
                    ComputeScore(current)
                ));
            }
            return;
        }

        foreach (string letter in pools[index])
        {
            if (string.IsNullOrEmpty(letter))
                continue;

            if (letter == "?")
            {
                for (char c = 'A'; c <= 'Z'; c++)
                {
                    GenerateCombinations(
                        pools,
                        index + 1,
                        current + c,
                        results
                    );
                }
            }
            if (letter == "-")
            {
                GenerateCombinations(
                    pools,
                    index + 1,
                    current,
                    results
                );
            }
            else
            {
                GenerateCombinations(
                    pools,
                    index + 1,
                    current + letter,
                    results
                );
            }
        }
    }

    private int ComputeScore(string word)
    {
        return LetterRules.ComputeWordScore(word);
    }

    private void SpawnButton(WordResult result)
    {
        WordButton button = Instantiate(
            wordButtonPrefab,
            resultsPanel
        );

        button.Setup(result, pawn, this);
        spawnedButtons.Add(button.gameObject);
    }

    private void ClearResults()
    {
        foreach (GameObject go in spawnedButtons)
            Destroy(go);

        spawnedButtons.Clear();
    }

    public void ConsumeCards()
    {
        foreach (DropZone zone in zones)
        {
            if (!zone.HasCard())
                continue;

            Card card = zone.GetCard();

            zone.ClearCard();   // visual / logical clear
            card.Discard();    // triggers DeckManager
        }

        ClearResults();
    }
}

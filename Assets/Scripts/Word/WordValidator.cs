using System.Collections;
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
    [SerializeField] private float consumeDelay = 0.08f;


    [Header("Gameplay")]
    [SerializeField] private Pawn pawn;

    private WordDictionary dictionary;
    private DropZone[] zones;
    private readonly List<GameObject> spawnedButtons = new();

    private readonly List<Symbol> pulsingSymbols = new();

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
        WordButton.OnWordHovered += HandleWordHovered;
        WordButton.OnWordUnhovered += ClearPulses;

        if (zones == null) return;

        foreach (DropZone zone in zones)
            zone.OnZoneChanged += Validate;
    }

    private void OnDisable()
    {
        WordButton.OnWordHovered -= HandleWordHovered;
        WordButton.OnWordUnhovered -= ClearPulses;

        if (zones == null) return;

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

        button.Setup(result, pawn, this, DialogueManager.Instance.currentSpeaker != null);
        spawnedButtons.Add(button.gameObject);
    }

    private Transform CreateConsumedPanel()
    {
        // Clone results panel
        GameObject panelGO = Instantiate(
            resultsPanel.gameObject,
            resultsPanel.parent
        );

        panelGO.name = "ConsumedResultsPanel";

        Transform panel = panelGO.transform;

        // Clear layout-driven size changes
        RectTransform rt = panel as RectTransform;
        RectTransform src = resultsPanel as RectTransform;

        rt.anchorMin = src.anchorMin;
        rt.anchorMax = src.anchorMax;
        rt.pivot = src.pivot;
        rt.anchoredPosition = src.anchoredPosition;
        rt.sizeDelta = src.sizeDelta;
        rt.localScale = Vector3.one;

        // Remove existing children (clone copies them)
        foreach (Transform child in panel)
            Destroy(child.gameObject);

        return panel;
    }

    private void ClearResults()
    {
        if (spawnedButtons.Count == 0)
            return;

        Transform consumedPanel = CreateConsumedPanel();

        foreach (GameObject go in spawnedButtons)
        {
            go.transform.SetParent(consumedPanel, worldPositionStays: false);

            WordButton wb = go.GetComponent<WordButton>();
            if (wb != null)
                wb.AnimateOutAndDestroy();
            else
                Destroy(go);
        }

        spawnedButtons.Clear();

        // Destroy the panel after animations are done
        StartCoroutine(DestroyPanelWhenEmpty(consumedPanel));
    }

    private IEnumerator DestroyPanelWhenEmpty(Transform panel)
    {
        // Wait until all buttons are gone
        while (panel.childCount > 0)
            yield return null;

        Destroy(panel.gameObject);
    }

    public void ConsumeCards()
    {
        StartCoroutine(ConsumeCardsRoutine());
    }

    private IEnumerator ConsumeCardsRoutine()
    {
        // Prevent new validations during consumption
        ClearResults();

        foreach (DropZone zone in zones)
        {
            if (!zone.HasCard())
                continue;

            zone.GetCard().Discard();

            yield return new WaitForSeconds(consumeDelay);
        }
    }

    private void HandleWordHovered(string word)
    {
        ClearPulses();

        int wordIndex = 0;

        foreach (DropZone zone in zones)
        {
            if (!zone.HasCard())
                continue;

            if (wordIndex >= word.Length)
                break;

            Card card = zone.GetCard();

            // Determine which letters this zone can contribute
            string[] zoneLetters = card.cardData._isFront ? card.cardData._frontFace : card.cardData._backFace;

            // If this zone only has "-" it contributes NOTHING
            bool contributesLetter = false;
            foreach (string l in zoneLetters)
            {
                if (l != "-" && l != "")
                {
                    contributesLetter = true;
                    break;
                }
            }

            if (!contributesLetter)
                continue; // DO NOT advance wordIndex

            string targetLetter = word[wordIndex].ToString();

            foreach (Symbol symbol in card.GetSymbols())
            {
                if (symbol.Letter == targetLetter)
                {
                    symbol.StartPulse();
                    pulsingSymbols.Add(symbol);
                    break;
                }
            }

            // ONLY advance when a letter was consumed
            wordIndex++;
        }
    }

    private void ClearPulses()
    {
        foreach (Symbol symbol in pulsingSymbols)
            symbol.StopPulse();

        pulsingSymbols.Clear();
    }
}

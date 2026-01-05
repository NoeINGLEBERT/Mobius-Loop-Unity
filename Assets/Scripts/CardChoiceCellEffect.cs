using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CardChoiceCellEffect : MonoBehaviour, ICellEffect
{
    [Header("Prefabs")]
    [SerializeField] private GameObject panelPrefab;   // Choice panel prefab
    [SerializeField] private GameObject cardPrefab;    // Card prefab

    [Header("Settings")]
    [SerializeField] private int numberOfChoices = 3;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private int cardTargetPoints = 10;

    public IEnumerator Activate(Pawn pawn)
    {
        if (!pawn.GetComponent<PlayerActor>()) yield break;

        // Find the Canvas automatically
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[CardChoiceCellEffect] No Canvas found in the scene.");
            yield break;
        }

        // Find DeckGenerator automatically
        DeckGenerator deckGenerator = FindFirstObjectByType<DeckGenerator>();
        if (deckGenerator == null)
        {
            Debug.LogError("[CardChoiceCellEffect] No DeckGenerator found in the scene.");
            yield break;
        }

        // Spawn choice panel
        GameObject panelGO = Object.Instantiate(panelPrefab, canvas.transform);
        CanvasGroup panelCG = panelGO.GetComponent<CanvasGroup>();
        if (panelCG == null)
            panelCG = panelGO.AddComponent<CanvasGroup>();
        panelCG.alpha = 0f;

        // Find the Horizontal Layout container automatically
        Transform container = panelGO.GetComponentInChildren<HorizontalLayoutGroup>()?.transform;
        if (container == null)
        {
            Debug.LogError("[CardChoiceCellEffect] Panel prefab must have a child with HorizontalLayoutGroup.");
            yield break;
        }

        Card chosenCard = null;
        bool cardChosen = false;

        // Spawn new cards under the container
        for (int i = 0; i < numberOfChoices; i++)
        {
            CardData data = deckGenerator.GenerateCard(cardTargetPoints);

            GameObject cardGO = Object.Instantiate(cardPrefab, container);
            Card card = cardGO.GetComponent<Card>();
            card.cardData = data;

            // Add CardButton if not present
            CardButton button = cardGO.GetComponent<CardButton>();
            if (button == null)
                button = cardGO.AddComponent<CardButton>();

            button.OnCardClicked += (c) =>
            {
                if (!cardChosen)
                {
                    chosenCard = c;
                    cardChosen = true;
                }
            };
        }

        // Fade in panel
        yield return FadeCanvasGroup(panelCG, 0f, 1f, fadeDuration);

        // Wait until a card is clicked
        while (!cardChosen)
            yield return null;

        // Discard chosen card
        chosenCard.Discard();

        // Destroy all other cards
        foreach (Transform child in container)
        {
            Card card = child.GetComponent<Card>();
            if (card != chosenCard)
                card.Destroy();
        }

        yield return null;

        // Fade out panel
        yield return FadeCanvasGroup(panelCG, 1f, 0f, fadeDuration);

        Object.Destroy(panelGO);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        cg.alpha = to;
    }
}

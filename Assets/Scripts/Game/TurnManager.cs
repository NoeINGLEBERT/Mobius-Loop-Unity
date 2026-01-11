using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> actorBehaviours;

    private readonly List<ITurnActor> actors = new();
    private int currentIndex;

    private int extraTurns;
    private int skipTurns;

    void Awake()
    {
        foreach (var mb in actorBehaviours)
            actors.Add((ITurnActor)mb);
    }

    void Start()
    {
        Debug.Log("TURN MANAGER STARTED");
        StartCoroutine(WaitForDeckThenStart());
    }

    private IEnumerator WaitForDeckThenStart()
    {
        // Wait until DeckManager exists
        while (DeckManager.Instance == null)
            yield return null;

        // Wait until deck is populated
        while (DeckManager.Instance.HandCount == 0)
            yield return null;

        Debug.Log("[TURN] Deck ready, starting turns");
        StartCoroutine(TurnLoop());
    }

    private IEnumerator TurnLoop()
    {
        while (true)
        {
            ITurnActor actor = actors[currentIndex];
            string actorName = ((MonoBehaviour)actor).name;

            // =========================
            // SKIP TURN
            // =========================
            if (skipTurns > 0)
            {
                Debug.Log(
                    $"[TURN] Skipping turn of {actorName} " +
                    $"(Index {currentIndex}) | Remaining skips: {skipTurns}"
                );

                skipTurns--;
                AdvanceIndex();
                continue;
            }

            // =========================
            // START TURN
            // =========================
            Debug.Log(
                $"[TURN] START: {actorName} " +
                $"(Index {currentIndex}) | ExtraTurns={extraTurns}"
            );

            yield return actor.TakeTurn();

            // =========================
            // END TURN
            // =========================
            Debug.Log(
                $"[TURN] END: {actorName} " +
                $"(Index {currentIndex})"
            );

            // =========================
            // EXTRA TURN
            // =========================
            if (extraTurns > 0)
            {
                extraTurns--;

                Debug.Log(
                    $"[TURN] Extra turn for {actorName} " +
                    $"(Index {currentIndex}) | Remaining extra: {extraTurns}"
                );

                continue; // same actor again
            }

            AdvanceIndex();
        }
    }

    private void AdvanceIndex()
    {
        currentIndex = (currentIndex + 1) % actors.Count;

        Debug.Log(
            $"[TURN] Next actor index: {currentIndex} " +
            $"({((MonoBehaviour)actors[currentIndex]).name})"
        );
    }

    // =========================
    // API FOR CELL EFFECTS
    // =========================

    public void GrantExtraTurn(int amount = 1)
    {
        extraTurns += amount;
        Debug.Log($"[TURN] GrantExtraTurn({amount}) : Total extra: {extraTurns}");
    }

    public void SkipNextTurns(int amount = 1)
    {
        skipTurns += amount;
        Debug.Log($"[TURN] SkipNextTurns({amount}) : Total skips: {skipTurns}");
    }
}

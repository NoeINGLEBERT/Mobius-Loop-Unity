using UnityEngine;
using System.Collections;

public class PlayerActor : MonoBehaviour, ITurnActor
{
    [SerializeField] private Pawn pawn;
    [SerializeField] private DeckManager deckManager;

    public Pawn Pawn => pawn;

    public IEnumerator TakeTurn()
    {
        // === TURN START ===
        deckManager.RefillHand();

        // === TURN END CONDITION ===
        yield return new WaitUntil(() =>
            deckManager.HandCount == 0 &&
            pawn.IsIdle
        );
    }
}

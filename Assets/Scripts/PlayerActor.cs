using UnityEngine;
using System.Collections;

public class PlayerActor : MonoBehaviour, ITurnActor
{
    [SerializeField] private Pawn pawn;
    [SerializeField] private DeckManager deckManager;

    public Pawn Pawn => pawn;

    private void OnEnable()
    {
        Card.OnFaceSwap += Pawn.Swap;
    }

    private void OnDisable()
    {
        Card.OnFaceSwap -= Pawn.Swap;
    }

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

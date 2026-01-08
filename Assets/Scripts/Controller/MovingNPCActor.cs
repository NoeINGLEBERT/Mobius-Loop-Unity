using UnityEngine;
using System.Collections;
public class MovingNPCActor : MonoBehaviour, ITurnActor
{
    [SerializeField] private Pawn pawn;
    [SerializeField] private int minMove = 1;
    [SerializeField] private int maxMove = 6;

    public Pawn Pawn => pawn;

    public IEnumerator TakeTurn()
    {
        int roll = Random.Range(minMove, maxMove + 1);
        pawn.MoveUpCells(roll, 0.4f);

        // Wait until movement + cell effect finishes
        yield return new WaitUntil(() => pawn.IsIdle);
    }
}


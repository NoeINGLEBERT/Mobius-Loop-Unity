using UnityEngine;
using System.Collections;

public interface ITurnActor
{
    Pawn Pawn { get; }
    IEnumerator TakeTurn();
}


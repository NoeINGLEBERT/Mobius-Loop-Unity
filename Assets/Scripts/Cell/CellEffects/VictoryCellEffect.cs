using System.Collections;
using UnityEngine;

[System.Serializable]
public class VictoryCellEffect : MonoBehaviour, ICellEffect
{
    public IEnumerator Activate(Pawn pawn)
    {
        if (!pawn.GetComponent<PlayerActor>())
            yield break;

        // =========================
        // VICTORY
        // =========================
        GameStateManager.Instance.TriggerVictory();
    }
}

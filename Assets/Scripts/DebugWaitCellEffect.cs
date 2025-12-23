using System.Collections;
using UnityEngine;

[System.Serializable]
public class DebugWaitCellEffect : MonoBehaviour, ICellEffect
{
    [SerializeField] private string message;
    [SerializeField] private float waitTime;

    public DebugWaitCellEffect(string message, float waitTime = 3f)
    {
        this.message = message;
        this.waitTime = waitTime;
    }

    public IEnumerator Activate(Pawn pawn)
    {
        Debug.Log($"[CellEffect] {message}");
        yield return new WaitForSeconds(waitTime);
    }
}

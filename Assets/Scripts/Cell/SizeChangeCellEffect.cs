using System.Collections;
using UnityEngine;

public enum SizeChangeType
{
    Shrink,
    Grow
}

[System.Serializable]
public class SizeChangeCellEffect : MonoBehaviour, ICellEffect
{
    [Header("Size Effect")]
    [SerializeField] private SizeChangeType sizeChange;

    public IEnumerator Activate(Pawn pawn)
    {
        // Only affect the player
        if (!pawn.TryGetComponent<PlayerActor>(out _))
            yield break;

        // Find WordValidator (authoritative size manager)
        WordValidator validator = WordValidator.Instance;
        if (validator == null)
        {
            Debug.LogWarning(
                "[CardSizeChangeCellEffect] No WordValidator found on player."
            );
            yield break;
        }

        // Apply effect
        switch (sizeChange)
        {
            case SizeChangeType.Shrink:
                validator.Shrink();
                break;

            case SizeChangeType.Grow:
                validator.Grow();
                break;
        }

        yield return null;
    }
}

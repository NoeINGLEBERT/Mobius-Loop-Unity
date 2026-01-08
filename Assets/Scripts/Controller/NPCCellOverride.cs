using UnityEngine;

public class NPCCellOverride : MonoBehaviour
{
    [SerializeField] private Pawn pawn;
    [SerializeField] private DialogueCellEffect npcDialogueEffect;

    private Cell currentCell;
    private ICellEffect savedEffect;
    private bool savedEvenLap;

    void Awake()
    {
        pawn.OnCellResolved += OnPawnLanded;
    }

    void OnDestroy()
    {
        pawn.OnCellResolved -= OnPawnLanded;
    }

    private void OnPawnLanded(Cell cell)
    {
        // Restore previous cell before hijacking a new one
        RestorePreviousCell();

        bool evenLap = pawn.board.IsEvenLap(pawn.playerData.cellIndex);

        currentCell = cell;
        savedEvenLap = evenLap;
        savedEffect = cell.GetEffect(evenLap);

        cell.SetEffect(evenLap, npcDialogueEffect);
    }

    public void RestorePreviousCell()
    {
        if (currentCell == null || savedEffect == null)
            return;

        currentCell.SetEffect(savedEvenLap, savedEffect);

        currentCell = null;
        savedEffect = null;
    }
}

using UnityEngine;

public class Pawn : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Board board;

    void Start()
    {
        MoveToCell(playerData.cellIndex);
    }

    void MoveToCell(int cellIndex)
    {
        int index = cellIndex % board.cellNumber;
        int nextIndex = (cellIndex + 1) % board.cellNumber;

        Transform cell = board.cells[index].transform;
        Transform nextCell = board.cells[nextIndex].transform;

        // Forward direction = toward the next cell
        Vector3 forward = (nextCell.position - cell.position).normalized;

        // Alternate "up" depending on lap parity because its a Möbius strip
        bool evenLap = ((cellIndex / board.cellNumber) % 2) == 0;
        Vector3 up = evenLap ? cell.up : -cell.up;

        transform.position = cell.position;
        transform.rotation = Quaternion.LookRotation(forward, up);
    }

    private void Update()
    {
        MoveToCell((int)(24 * Time.time - (24 * Time.time % 1f)));
    }
}

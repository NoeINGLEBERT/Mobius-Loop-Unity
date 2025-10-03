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
        transform.position = board.cells[cellIndex % board.cellNumber].transform.position;
        transform.rotation = board.cells[cellIndex % board.cellNumber].transform.rotation;
    }
}

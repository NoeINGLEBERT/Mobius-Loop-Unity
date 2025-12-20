using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int cellNumber;
    [SerializeField] private GameObject cell;
    public GameObject[] cells;
    [SerializeField] private float radius;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cells = new GameObject[cellNumber];
        for (int i = 0; i < cellNumber; i++)
        {
            float alpha = (float)i / (float)cellNumber;
            cells[i] = Instantiate(cell);
            cells[i].transform.position = new Vector3(radius, 0f, 0f);
            cells[i].transform.RotateAround(Vector3.zero, Vector3.up, alpha  * 360);
            Vector3 eulerAngles = cells[i].transform.rotation.eulerAngles;
            eulerAngles.z = alpha * 180;
            cells[i].transform.rotation = Quaternion.Euler(eulerAngles);
        }
    }

    public void GetCellTransform(int cellIndex, out Vector3 position, out Quaternion rotation)
    {
        int index = cellIndex % cellNumber;
        int nextIndex = (cellIndex + 1) % cellNumber;

        Transform cell = cells[index].transform;
        Transform nextCell = cells[nextIndex].transform;

        // Forward direction = toward the next cell
        Vector3 forward = (nextCell.position - cell.position).normalized;

        // Alternate "up" depending on lap parity because its a Möbius strip
        bool evenLap = ((cellIndex / cellNumber) % 2) == 0;
        Vector3 up = evenLap ? cell.up : -cell.up;

        position = cell.position;
        rotation = Quaternion.LookRotation(forward, up);
    }
}
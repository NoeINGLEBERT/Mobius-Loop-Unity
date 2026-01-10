using System.Linq;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CellRule
{
    public GameObject prefab;

    [Tooltip("How many times this cell should appear")]
    public int count;

    [Tooltip("Random offset inside its segment (0 = fixed center, 1 = full randomness)")]
    [Range(0f, 1f)]
    public float randomness;
}

public class Board : MonoBehaviour
{
    [Header("Board Shape")]
    [SerializeField] public int cellNumber;
    [SerializeField] private float radius;

    [Header("Cell Prefabs")]
    [SerializeField] private GameObject defaultCell;
    [SerializeField] private GameObject firstCell;

    [Header("Special Cells")]
    [SerializeField] private List<CellRule> specialCells;

    public GameObject[] cells;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        cells = new GameObject[cellNumber];

        // 1 Decide which prefab goes where
        GameObject[] layout = BuildCellLayout();

        // 2 Instantiate & position
        for (int i = 0; i < cellNumber; i++)
        {
            float alpha = (float)i / cellNumber;

            GameObject cellGO = Instantiate(layout[i], transform);

            cellGO.transform.position = new Vector3(radius, 0f, 0f);
            cellGO.transform.RotateAround(Vector3.zero, Vector3.up, alpha * 360f);

            Vector3 euler = cellGO.transform.rotation.eulerAngles;
            euler.z = alpha * 180f;
            cellGO.transform.rotation = Quaternion.Euler(euler);

            cells[i] = cellGO;
        }
    }

    private GameObject[] BuildCellLayout()
    {
        GameObject[] layout = new GameObject[cellNumber];

        // Default fill
        for (int i = 0; i < cellNumber; i++)
            layout[i] = defaultCell;

        // First cell override
        if (firstCell != null)
            layout[0] = firstCell;

        HashSet<int> occupied = new() { 0 };

        foreach (CellRule rule in specialCells)
        {
            if (rule.prefab == null || rule.count <= 0)
                continue;

            PlaceStratified(layout, occupied, rule);
        }

        return layout;
    }

    private void PlaceStratified(GameObject[] layout, HashSet<int> occupied, CellRule rule)
    {
        float segmentSize = (float)cellNumber / rule.count;

        for (int i = 0; i < rule.count; i++)
        {
            int segmentStart = Mathf.FloorToInt(i * segmentSize);
            int segmentEnd = Mathf.FloorToInt((i + 1) * segmentSize) - 1;

            segmentEnd = Mathf.Clamp(segmentEnd, 0, cellNumber - 1);

            int attempts = 10;
            int index = segmentStart;

            while (attempts-- > 0)
            {
                float t = Random.value * rule.randomness;
                index = Mathf.RoundToInt(
                    Mathf.Lerp(segmentStart, segmentEnd, t)
                );

                if (!occupied.Contains(index))
                    break;
            }

            if (occupied.Contains(index))
                continue;

            layout[index] = rule.prefab;
            occupied.Add(index);
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
        bool evenLap = IsEvenLap(cellIndex);
        Vector3 up = evenLap ? cell.up : -cell.up;

        position = cell.position;
        rotation = Quaternion.LookRotation(forward, up);
    }

    public Cell GetCell(int cellIndex)
    {
        int index = cellIndex % cellNumber;
        return cells[index].GetComponent<Cell>();
    }

    public bool IsEvenLap(int cellIndex)
    {
        return ((cellIndex / cellNumber) % 2) == 0;
    }
}
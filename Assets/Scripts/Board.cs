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
}

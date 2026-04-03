using UnityEngine;

public class FlowerSurfaceSpawner : MonoBehaviour
{
    public GameObject flowerPrefab;
    public MeshRenderer floorRenderer;

    public int attempts = 5000;

    public LayerMask surfaceMask; // Floor + Walls

    void Start()
    {
        Bounds bounds = floorRenderer.bounds;

        for (int i = 0; i < attempts; i++)
        {
            Vector3 origin = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.max.y + 5f,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 20f, surfaceMask))
            {
                SpawnOnSurface(hit);
            }
        }
    }

    void SpawnOnSurface(RaycastHit hit)
    {
        Vector3 pos = hit.point + hit.normal * 0.02f;

        // Align with surface normal
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);

        // Optional: random twist around normal
        rot *= Quaternion.AngleAxis(Random.Range(0f, 360f), hit.normal);

        Instantiate(flowerPrefab, pos, rot, transform);
    }
}
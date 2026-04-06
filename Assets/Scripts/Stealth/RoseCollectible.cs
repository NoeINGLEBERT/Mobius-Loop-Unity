using UnityEngine;

public class RoseCollectible : MonoBehaviour
{
    private Renderer[] renderers;   // all renderers in children
    public Color pickedColor = Color.red;

    private bool collected = false;

    void Start()
    {
        // Grab every renderer inside the prefab (petals, leaves, etc.)
        renderers = GetComponentsInChildren<Renderer>();

        // Start white
        SetColor(Color.white);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TEST");

        if (collected) return;

        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        collected = true;

        // Turn whole rose red
        SetColor(pickedColor);

        // Give point
        ScoreManager.Instance.AddPoint(1);

        // Optional: disable collider so it can't be picked twice
        GetComponent<Collider>().enabled = false;
    }

    void SetColor(Color color)
    {
        foreach (Renderer rend in renderers)
        {
            // material = instance at runtime (safe to modify)
            rend.material.color = color;
        }
    }
}
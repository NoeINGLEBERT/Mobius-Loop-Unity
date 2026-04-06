using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// ================= Level Load Effect =================
[CreateAssetMenu(fileName = "LevelLoadEffect", menuName = "Cell Effects/Level Load")]
public class LevelLoadEffect : MonoBehaviour, ICellEffect
{
    [Header("Level Load Settings")]
    public string sceneName;            // Scene to load
    public float delay = 0f;            // Optional delay before loading
    public GameObject widgetPrefab;     // Optional "You've been spotted" UI

    public IEnumerator Activate(Pawn pawn)
    {
        // Spawn UI widget if assigned
        if (widgetPrefab != null)
        {
            Instantiate(widgetPrefab);
        }

        // Optional delay before level load
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // Load the new scene
        SceneManager.LoadScene(sceneName);
    }
}
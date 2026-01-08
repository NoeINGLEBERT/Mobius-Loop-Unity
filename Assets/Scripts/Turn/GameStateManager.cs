using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        Victory,
        Defeat
    }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("UI")]
    [SerializeField] private CanvasGroup victoryPanel;
    [SerializeField] private CanvasGroup defeatPanel;
    [SerializeField] private float fadeDuration = 0.4f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // =========================
    // PUBLIC API
    // =========================

    public void TriggerVictory()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.Victory;
        ShowPanel(victoryPanel);
    }

    public void TriggerDefeat()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.Defeat;
        ShowPanel(defeatPanel);
    }

    // =========================
    // INTERNAL
    // =========================

    private void ShowPanel(CanvasGroup panel)
    {
        panel.gameObject.SetActive(true);
        StartCoroutine(FadeCanvas(panel, 0f, 1f));

        Time.timeScale = 0f; // Pause game
    }

    private System.Collections.IEnumerator FadeCanvas(CanvasGroup cg, float from, float to)
    {
        float t = 0f;
        cg.alpha = from;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        cg.alpha = to;
    }

    // =========================
    // BUTTON HOOKS
    // =========================

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

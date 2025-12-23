using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public struct WordResult
{
    public string word;
    public int score;

    public WordResult(string word, int score)
    {
        this.word = word;
        this.score = score;
    }
}

public class WordButton : MonoBehaviour
{
    [SerializeField] TMP_Text wordText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] Button button;

    [Header("Animation")]
    [SerializeField] private float appearDuration = 0.15f;
    [SerializeField] private float disappearDuration = 0.12f;
    [SerializeField] private float clickDisappearDuration = 0.22f;
    [SerializeField] private float clickPunchScale = 1.1f;

    private WordResult result;
    private Pawn pawn;
    private WordValidator validator;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    private bool exiting = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        rect.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
    }

    public void Setup(WordResult result, Pawn pawn, WordValidator validator)
    {
        this.result = result;
        this.pawn = pawn;
        this.validator = validator;

        wordText.text = result.word;
        scoreText.text = result.score.ToString();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);

        StartCoroutine(Appear());
    }

    private void OnClicked()
    {
        button.interactable = false;

        AnimateOutAndDestroy(clicked: true);

        validator.ConsumeCards();
        pawn.MoveUpCells(result.score, 1f);
    }

    // =========================
    // ANIMATIONS
    // =========================

    IEnumerator Appear()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / appearDuration;
            rect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, EaseOut(t));
            canvasGroup.alpha = t;
            yield return null;
        }

        rect.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }

    public void AnimateOutAndDestroy(bool clicked = false)
    {
        if (exiting) return;

        exiting = true;
        StopAllCoroutines();

        StartCoroutine(ExitRoutine(clicked));
    }

    IEnumerator ExitRoutine(bool clicked)
    {
        float duration = clicked ? clickDisappearDuration : disappearDuration;

        float t = 0f;

        Vector3 startScale = rect.localScale;
        Vector3 endScale = clicked ? Vector3.one * clickPunchScale : Vector3.zero;

        float startAlpha = canvasGroup.alpha;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = EaseOut(t);

            rect.localScale = Vector3.Lerp(startScale, endScale, eased);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        yield return new WaitForSeconds(clickDisappearDuration - duration);

        Destroy(gameObject);
    }

    // =========================
    // EASING
    // =========================

    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3);
    }
}

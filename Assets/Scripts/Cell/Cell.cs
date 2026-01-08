using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private MonoBehaviour evenLapEffect;
    [SerializeField] private MonoBehaviour oddLapEffect;

    private ICellEffect evenEffect;
    private ICellEffect oddEffect;

    void Awake()
    {
        evenEffect = evenLapEffect as ICellEffect;
        oddEffect = oddLapEffect as ICellEffect;

        if (evenLapEffect && evenEffect == null)
            Debug.LogError($"{name}: EvenLapEffect does not implement ICellEffect");

        if (oddLapEffect && oddEffect == null)
            Debug.LogError($"{name}: OddLapEffect does not implement ICellEffect");
    }

    public IEnumerator Activate(Pawn pawn, bool evenLap)
    {
        ICellEffect effect = evenLap ? evenEffect : oddEffect;

        if (effect != null)
            yield return effect.Activate(pawn);
    }
    public ICellEffect GetEffect(bool evenLap)
    {
        return evenLap ? evenEffect : oddEffect;
    }

    public void SetEffect(bool evenLap, ICellEffect effect)
    {
        if (evenLap)
            evenEffect = effect;
        else
            oddEffect = effect;
    }
}

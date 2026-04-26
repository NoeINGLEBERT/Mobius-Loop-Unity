using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // or UnityEngine.UI if using legacy Text
public class CupGameManager : MonoBehaviour
{
    public List<Cup> cups;
    public Transform ball;

    public float moveDuration = 0.6f;
    public int shuffleCount = 10;
    public float arcDepth = 1.5f;
    public float liftHeight = 2f;

    private Cup ballCup;
    private List<Vector3> slotPositions = new List<Vector3>();

    public Deck deck;
    public TMP_Text victoryText; // assign in inspector
    public string nextSceneName;

    enum MoveType
    {
        Linear,
        ArcFront,
        ArcBack
    }

    void Start()
    {
        CacheSlots();
        SetupCups();
        StartCoroutine(GameRoutine());
    }

    void CacheSlots()
    {
        slotPositions.Clear();

        foreach (var cup in cups)
            slotPositions.Add(cup.transform.position);
    }

    void SetupCups()
    {
        for (int i = 0; i < cups.Count; i++)
        {
            cups[i].Init(this);
            cups[i].transform.position = slotPositions[i];
        }

        ballCup = cups[Random.Range(0, cups.Count)];
        ball.SetParent(ballCup.ballAnchor);
        ball.localPosition = Vector3.zero;
    }

    IEnumerator GameRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        yield return RevealAllCups();

        yield return new WaitForSeconds(0.5f);

        yield return ShuffleCups();

        EnableSelection(true);
    }

    IEnumerator RevealAllCups()
    {
        foreach (var cup in cups)
            StartCoroutine(LiftCup(cup, true));

        yield return new WaitForSeconds(moveDuration + 0.3f);

        foreach (var cup in cups)
            StartCoroutine(LiftCup(cup, false));

        yield return new WaitForSeconds(moveDuration);
    }

    IEnumerator ShuffleCups()
    {
        for (int i = 0; i < shuffleCount; i++)
        {
            yield return StartCoroutine(ShuffleStep());
        }
    }

    IEnumerator ShuffleStep()
    {
        int count = cups.Count;

        // 1. Generate unique target slots
        List<int> newIndices = GenerateDerangement(count);

        // 2. Assign unique movement types
        List<MoveType> moveTypes = new List<MoveType>()
        {
            MoveType.Linear,
            MoveType.ArcFront,
            MoveType.ArcBack
        };

        ShuffleList(moveTypes);

        // 3. Cache positions
        Vector3[] startPos = new Vector3[count];
        Vector3[] targetPos = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            startPos[i] = cups[i].transform.position;
            targetPos[i] = slotPositions[newIndices[i]];
        }

        float time = 0;

        while (time < moveDuration)
        {
            float t = time / moveDuration;
            t = Mathf.SmoothStep(0, 1, t);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos;

                bool isStatic = (startPos[i] == targetPos[i]);

                MoveType moveType = moveTypes[i];

                if (isStatic)
                    moveType = MoveType.Linear;

                switch (moveType)
                {
                    case MoveType.Linear:
                        pos = Vector3.Lerp(startPos[i], targetPos[i], t);
                        break;

                    case MoveType.ArcFront:
                        pos = Vector3.Lerp(startPos[i], targetPos[i], t);
                        pos += Vector3.forward * Mathf.Sin(t * Mathf.PI) * arcDepth;
                        break;

                    case MoveType.ArcBack:
                        pos = Vector3.Lerp(startPos[i], targetPos[i], t);
                        pos += Vector3.back * Mathf.Sin(t * Mathf.PI) * arcDepth;
                        break;

                    default:
                        pos = startPos[i];
                        break;
                }

                cups[i].transform.position = pos;
            }

            time += Time.deltaTime;
            yield return null;
        }

        // 4. Snap to exact slots
        for (int i = 0; i < count; i++)
        {
            cups[i].transform.position = targetPos[i];
        }

        // 5. Apply new order
        ApplyPermutation(newIndices);
    }

    List<int> GenerateDerangement(int count)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < count; i++)
            result.Add(i);

        bool valid = false;

        // retry until no index matches
        while (!valid)
        {
            // shuffle
            for (int i = 0; i < count; i++)
            {
                int j = Random.Range(i, count);
                int temp = result[i];
                result[i] = result[j];
                result[j] = temp;
            }

            valid = true;

            for (int i = 0; i < count; i++)
            {
                if (result[i] == i)
                {
                    valid = false;
                    break;
                }
            }
        }

        return result;
    }

    void ApplyPermutation(List<int> newIndices)
    {
        List<Cup> newOrder = new List<Cup>(cups);

        for (int i = 0; i < cups.Count; i++)
        {
            newOrder[newIndices[i]] = cups[i];
        }

        cups = newOrder;

        // 🔥 Update ball tracking automatically
        // (ballCup is attached to the Cup object, so no change needed)
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    void EnableSelection(bool enable)
    {
        foreach (var cup in cups)
            cup.SetClickable(enable);
    }

    public void OnCupSelected(Cup selectedCup)
    {
        EnableSelection(false);
        StartCoroutine(Reveal(selectedCup));
    }

    IEnumerator Reveal(Cup selectedCup)
    {
        yield return LiftCup(selectedCup, true);

        bool isCorrect = (selectedCup == ballCup);

        if (!isCorrect)
        {
            yield return new WaitForSeconds(1f);
            yield return LiftCup(ballCup, true);
        }

        // 🔥 UPDATE UI
        victoryText.text = isCorrect ? "YOU DID IT!! YOU MANAGED TO KEEP YOUR SANITY" : "WRONG CUP, 3 CARDS LOST TO MADNESS";

        // 🔥 MODIFY DECK ON FAIL
        if (!isCorrect)
        {
            RemoveCardsFromDeck(3);
        }

        // 🔥 LOAD NEXT SCENE AFTER DELAY
        StartCoroutine(LoadNextSceneAfterDelay(5f));
    }

    void RemoveCardsFromDeck(int amount)
    {
        if (deck == null || deck.cards == null || deck.cards.Length == 0)
            return;

        int originalLength = deck.cards.Length;
        int removeCount = Mathf.Min(amount, originalLength);

        // Create a list of indices to remove
        List<int> indices = new List<int>();
        for (int i = 0; i < originalLength; i++)
            indices.Add(i);

        // Shuffle indices
        for (int i = 0; i < indices.Count; i++)
        {
            int j = Random.Range(i, indices.Count);
            int temp = indices[i];
            indices[i] = indices[j];
            indices[j] = temp;
        }

        // Mark which indices to remove
        HashSet<int> toRemove = new HashSet<int>();
        for (int i = 0; i < removeCount; i++)
            toRemove.Add(indices[i]);

        // Build new array
        CardData[] newCards = new CardData[originalLength - removeCount];

        int newIndex = 0;
        for (int i = 0; i < originalLength; i++)
        {
            if (!toRemove.Contains(i))
            {
                newCards[newIndex] = deck.cards[i];
                newIndex++;
            }
        }

        deck.cards = newCards;
    }

    IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator LiftCup(Cup cup, bool up)
    {
        Vector3 start = cup.transform.position;
        Vector3 end = up ? start + Vector3.up * liftHeight : start - Vector3.up * liftHeight;

        bool hasBall = (cup == ballCup);

        Vector3 ballWorldPos = Vector3.zero;

        // 🔥 DETACH BALL WHEN LIFTING
        if (hasBall && up)
        {
            ballWorldPos = ball.position;
            ball.SetParent(null); // detach from cup
        }

        float time = 0;

        while (time < moveDuration)
        {
            float t = time / moveDuration;
            t = Mathf.SmoothStep(0, 1, t);

            cup.transform.position = Vector3.Lerp(start, end, t);

            time += Time.deltaTime;
            yield return null;
        }

        cup.transform.position = end;

        // 🔥 REATTACH WHEN CUP GOES DOWN
        if (hasBall && !up)
        {
            ball.SetParent(cup.ballAnchor);
            ball.localPosition = Vector3.zero;
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PawnCommandType
{
    Move,
    Swap
}

public struct PawnCommand
{
    public PawnCommandType type;
    public int amount;          // for Move
    public float duration;      // total duration

    public PawnCommand(PawnCommandType type, int amount = 0, float duration = 0.5f)
    {
        this.type = type;
        this.amount = amount;
        this.duration = duration;
    }
}
public class Pawn : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private Board board;

    private readonly Queue<PawnCommand> commandQueue = new();
    private bool isExecuting;

    void Start()
    {
        SnapToCell(playerData.cellIndex);
    }

    private void OnEnable()
    {
        Card.OnFaceSwap += Swap;
    }

    private void OnDisable()
    {
        Card.OnFaceSwap -= Swap;
    }

    // =========================
    // PUBLIC API
    // =========================

    public void MoveUpCells(int cellCount, float duration)
    {
        Enqueue(new PawnCommand(PawnCommandType.Move, cellCount, duration));
    }

    public void Swap()
    {
        Enqueue(new PawnCommand(PawnCommandType.Swap));
    }

    // =========================
    // QUEUE SYSTEM
    // =========================

    private void Enqueue(PawnCommand command)
    {
        commandQueue.Enqueue(command);

        if (!isExecuting)
            StartCoroutine(ExecuteQueue());
    }

    private IEnumerator ExecuteQueue()
    {
        isExecuting = true;

        while (commandQueue.Count > 0)
        {
            PawnCommand cmd = commandQueue.Dequeue();

            switch (cmd.type)
            {
                case PawnCommandType.Move:
                    yield return MoveAnimated(cmd.amount, cmd.duration);
                    break;

                case PawnCommandType.Swap:
                    yield return SwapAnimated();
                    break;
            }
        }

        isExecuting = false;
    }

    // =========================
    // MOVEMENT
    // =========================

    private IEnumerator MoveAnimated(int cellCount, float totalDuration)
    {
        int steps = Mathf.Abs(cellCount);
        if (steps == 0)
            yield break;

        float stepDuration = totalDuration / steps;
        int direction = cellCount >= 0 ? 1 : -1;

        for (int i = 0; i < steps; i++)
        {
            int fromIndex = playerData.cellIndex;
            int toIndex = playerData.cellIndex + direction;

            yield return Hop(fromIndex, toIndex, stepDuration);

            playerData.cellIndex = toIndex;
        }

        yield return ResolveCellEffect();
    }

    private IEnumerator ResolveCellEffect()
    {
        Cell cell = board.GetCell(playerData.cellIndex);
        if (cell == null)
            yield break;

        bool evenLap = board.IsEvenLap(playerData.cellIndex);
        yield return cell.Activate(this, evenLap);
    }

    private IEnumerator Hop(int fromIndex, int toIndex, float duration)
    {
        board.GetCellTransform(fromIndex, out Vector3 startPos, out Quaternion startRot);
        board.GetCellTransform(toIndex, out Vector3 endPos, out Quaternion endRot);

        float height = 0.3f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = Mathf.Clamp01(t);

            // vertical hop in local "up" of the start rotation
            Vector3 up = startRot * Vector3.up;
            float yOffset = Mathf.Sin(eased * Mathf.PI) * height;

            transform.position =
                Vector3.Lerp(startPos, endPos, eased) +
                up * yOffset;

            transform.rotation =
                Quaternion.Slerp(startRot, endRot, eased);

            yield return null;
        }

        transform.SetPositionAndRotation(endPos, endRot);
    }


    // =========================
    // SWAP (MÖBIUS FLIP)
    // =========================

    private IEnumerator SwapAnimated()
    {
        float duration = 0.4f;
        float t = 0f;

        // logical swap = move half a loop forward
        int targetIndex = playerData.cellIndex + board.cellNumber;

        board.GetCellTransform(playerData.cellIndex, out _, out Quaternion startRot);
        board.GetCellTransform(targetIndex, out _, out Quaternion endRot);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;

            playerData.cellIndex = targetIndex;
        }

        transform.rotation = endRot;
    }

    // =========================
    // UTIL
    // =========================

    void SnapToCell(int cellIndex)
    {
        board.GetCellTransform(cellIndex, out Vector3 pos, out Quaternion rot);

        transform.SetPositionAndRotation(pos, rot);
    }
}

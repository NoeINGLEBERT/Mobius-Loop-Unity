using System.Collections;

public interface ICellEffect
{
    IEnumerator Activate(Pawn pawn);
}
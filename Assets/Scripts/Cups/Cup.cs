using UnityEngine;

public class Cup : MonoBehaviour
{
    public Transform ballAnchor;
    public GameObject clickableIndicator;

    private CupGameManager manager;
    private bool isClickable;

    public void Init(CupGameManager mgr)
    {
        manager = mgr;
    }

    public void SetClickable(bool value)
    {
        isClickable = value;

        if (clickableIndicator != null)
            clickableIndicator.SetActive(value);
    }

    private void OnMouseDown()
    {
        if (!isClickable) return;

        manager.OnCupSelected(this);
    }
}
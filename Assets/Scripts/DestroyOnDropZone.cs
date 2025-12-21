using UnityEngine;

[RequireComponent(typeof(DropZone))]
public class DestroyOnDropZone : MonoBehaviour
{
    private DropZone zone;

    private void Awake()
    {
        zone = GetComponent<DropZone>();
    }

    private void OnEnable()
    {
        zone.OnZoneChanged += OnZoneChanged;
    }

    private void OnDisable()
    {
        zone.OnZoneChanged -= OnZoneChanged;
    }

    private void OnZoneChanged()
    {
        if (!zone.HasCard())
            return;

        zone.GetCard().Destroy();
    }
}

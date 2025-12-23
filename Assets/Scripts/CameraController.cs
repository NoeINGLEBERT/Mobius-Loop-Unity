using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform pivot;

    [Header("Rotation")]
    [SerializeField] private float yawSpeed = 270f;
    [SerializeField] private float pitchSpeed = 360f;
    [SerializeField] private Vector2 pitchLimits = new(-80f, 80f);

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2.5f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 8f;

    [Header("Input")]
    [SerializeField] private int rotateMouseButton = 0; // 0 = Left Mouse

    private float yaw;
    private float pitch;
    private float distance;

    private bool rotating;

    void Start()
    {
        if (!pivot)
        {
            Debug.LogError("CameraController: No pivot assigned.");
            enabled = false;
            return;
        }

        // Initialize from actual camera transform
        Vector3 toCam = transform.position - pivot.position;
        distance = toCam.magnitude;

        Vector3 euler = transform.rotation.eulerAngles;
        pitch = NormalizeAngle(euler.x);
        yaw = NormalizeAngle(euler.y);

        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

        UpdateCameraTransform();
    }

    void Update()
    {
        HandleRotation();
        HandleZoom();
    }

    // =========================
    // ROTATION
    // =========================

    void HandleRotation()
    {
        if (rotating)
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            yaw += mx * yawSpeed * Time.deltaTime;
            pitch -= my * pitchSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

            UpdateCameraTransform();
        }

        if (Input.GetMouseButtonDown(rotateMouseButton))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            rotating = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetMouseButtonUp(rotateMouseButton))
        {
            rotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // =========================
    // ZOOM
    // =========================

    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        UpdateCameraTransform();
    }

    // =========================
    // APPLY TRANSFORM
    // =========================

    void UpdateCameraTransform()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * Vector3.back * distance;

        transform.position = pivot.position + offset;
        transform.rotation = rotation;
    }

    // =========================
    // UTILS
    // =========================

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}

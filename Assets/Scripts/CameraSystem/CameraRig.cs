using UnityEngine;

// Put this on any object in the scene. It finds the Main Camera and positions it itself.
// It frames the board automatically, rotates in 90 degree steps with Q and E,
// and exposes FocusOn / ReturnToOverview for anything that wants a camera shot.
public class CameraRig : MonoBehaviour
{
    public static CameraRig Instance { get; private set; }

    [Header("View")]
    public float pitch = 40f;
    public float startYaw = 45f;
    public float margin = 1.15f;       // spare room around the board
    public float smoothTime = 0.25f;

    [Header("Keys")]
    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;

    const float Distance = 60f;

    Camera cam;
    Bounds board;
    bool hasBoard;
    bool overview = true;
    float focusZoom = 1f;

    Vector3 focus, focusTarget, focusVel;
    float yaw, yawTarget, yawVel;
    float size, sizeVel;

    void Awake()
    {
        Instance = this;
        cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) { Debug.LogError("CameraRig could not find a Camera."); enabled = false; return; }

        cam.orthographic = true;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = Distance * 3f;

        yaw = yawTarget = startYaw;
    }

    void OnEnable() { GameEvents.OnLevelBuilt += OnLevelBuilt; }
    void OnDisable() { GameEvents.OnLevelBuilt -= OnLevelBuilt; }

    void OnLevelBuilt(Bounds b)
    {
        board = b;
        hasBoard = true;
        overview = true;
        focusZoom = 1f;
        focusTarget = b.center;
        focus = focusTarget;
        size = OverviewSize();
        focusVel = Vector3.zero;
        sizeVel = 0f;
        Apply();
    }

    // ---- Public camera controls ----

    // Move toward a point. zoom is a fraction of the full-board view where 0.6 is closer.
    public void FocusOn(Vector3 worldPoint, float zoom = 0.6f)
    {
        overview = false;
        focusTarget = worldPoint;
        focusZoom = Mathf.Clamp(zoom, 0.2f, 1f);
    }

    public void ReturnToOverview()
    {
        overview = true;
        focusZoom = 1f;
        if (hasBoard) focusTarget = board.center;
    }

    public void RotateSteps(int steps) { yawTarget += 90f * steps; }

    // ---- Internals ----

    void Update()
    {
        if (!hasBoard) return;

        if (Input.GetKeyDown(rotateLeftKey)) RotateSteps(-1);
        if (Input.GetKeyDown(rotateRightKey)) RotateSteps(1);

        float targetSize = OverviewSize() * (overview ? 1f : focusZoom);
        focus = Vector3.SmoothDamp(focus, focusTarget, ref focusVel, smoothTime);
        yaw = Mathf.SmoothDamp(yaw, yawTarget, ref yawVel, smoothTime);
        size = Mathf.SmoothDamp(size, targetSize, ref sizeVel, smoothTime);
        Apply();
    }

    void Apply()
    {
        // The camera is placed in world space, so it works wherever it sits in the hierarchy
        var rot = Quaternion.Euler(pitch, yaw, 0f);
        cam.transform.SetPositionAndRotation(focus - rot * Vector3.forward * Distance, rot);
        cam.orthographicSize = size;
    }

    // Smallest orthographic size that fits the whole board at any yaw
    float OverviewSize()
    {
        float r = Mathf.Sqrt(board.extents.x * board.extents.x + board.extents.z * board.extents.z);
        float p = pitch * Mathf.Deg2Rad;
        float verticalHalf = r * Mathf.Sin(p) + board.extents.y * Mathf.Cos(p);
        float horizontalHalf = r;
        return Mathf.Max(verticalHalf, horizontalHalf / cam.aspect) * margin;
    }
}
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    [Min(0.01f)] public float zoomSpeed = 20f;
    public float minCamSize = 1f;
    public float maxCamSize = 20f;

    private Camera cam;
    private Transform camT;

    // Panning variables
    private readonly float dragThresholdWorld = 0.2f; // min distance in world units to consider as drag
    private bool isPanning = false;
    private bool hasBegunPanning = false; // indicates if panning has truly started (exceeded threshold)

    private Vector3 initialMouseScreenPos;
    private Vector3 lastMouseScreenPos;

    void Awake()
    {
        cam = GetComponent<Camera>();
        camT = cam.transform;
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        scroll *= 100f; // scroll returns 0.1 or -0.1, this normalizes it so it feels sufficiently responsive 

        float oldSize = cam.orthographicSize;
        float newSize = Mathf.Clamp(oldSize - scroll * zoomSpeed * Time.deltaTime, minCamSize, maxCamSize);

        Vector3 worldBefore = cam.ScreenToWorldPoint(Input.mousePosition);
        cam.orthographicSize = newSize;
        Vector3 worldAfter = cam.ScreenToWorldPoint(Input.mousePosition);

        camT.position += (worldBefore - worldAfter);
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isPanning = true;
            hasBegunPanning = false; // reset for a new click
            initialMouseScreenPos = Input.mousePosition;
            lastMouseScreenPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) && isPanning)
        {
            Vector3 currentMouseScreenPos = Input.mousePosition;

            if (!hasBegunPanning)
            {
                // check if the drag threshold has been met in world units
                Vector3 initialMouseWorldPosAtClick = cam.ScreenToWorldPoint(initialMouseScreenPos);
                Vector3 currentMouseWorldPosAtClick = cam.ScreenToWorldPoint(currentMouseScreenPos); // this will change as camera moves, but for threshold it's fine.

                float currentDragDistanceWorld = Vector3.Distance(initialMouseWorldPosAtClick, currentMouseWorldPosAtClick);

                if (currentDragDistanceWorld >= dragThresholdWorld)
                {
                    hasBegunPanning = true;
                }
            }

            if (hasBegunPanning)
            {
                // Calculate the difference in screen space since the last frame
                Vector3 screenDelta = currentMouseScreenPos - lastMouseScreenPos;

                // Convert this screen delta to world coordinates.
                // We can't just convert screenDelta directly to world, because the world size
                // of a pixel depends on the camera's orthographic size.
                // A simpler way:
                // Get two world points corresponding to screen points one frame apart.
                Vector3 worldDelta = cam.ScreenToWorldPoint(Vector3.zero) - cam.ScreenToWorldPoint(screenDelta);

                // Apply this world delta to the camera's position.
                // We add because a positive screenX delta means the mouse moved right,
                // so the camera needs to move left to keep the content under the mouse.
                camT.position += worldDelta;
            }

            lastMouseScreenPos = currentMouseScreenPos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isPanning = false;
            hasBegunPanning = false; // ensure this resets
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        maxCamSize = Mathf.Max(maxCamSize, minCamSize);
    }
#endif
}
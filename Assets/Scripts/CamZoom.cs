using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    [Min(0.01f)] public float zoomSpeed = 20f;
    public float minCamSize = 5f;
    public float maxCamSize = 20f;

    private Camera cam;
    private Transform camT;

    private bool isDragging;
    private Vector2 clickWorldPos;

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

        // World‐point under mouse before/after zoom
        Vector3 worldBefore = cam.ScreenToWorldPoint(Input.mousePosition);
        cam.orthographicSize = newSize;
        Vector3 worldAfter = cam.ScreenToWorldPoint(Input.mousePosition);

        camT.position += (worldBefore - worldAfter);
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            clickWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging) return;

        Vector2 currentWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 delta = currentWorldPos - clickWorldPos;
        camT.position -= (Vector3)delta;
        // keep clickWorldPos constant so dragging feels smooth
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        maxCamSize = Mathf.Max(maxCamSize, minCamSize);
    }
#endif
}

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Orbit Settings")]
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float zoomSpeedMouse = 5f;
    [SerializeField] private float zoomSpeedTouch = 0.1f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 20f;

    private float currentDistance;
    private Vector3 lastMousePosition;
    private bool isDragging;

    void Start()
    {
        if (target == null)
            target = new GameObject("Camera Target").transform;

        currentDistance = Vector3.Distance(transform.position, target.position);
    }

    void Update()
    {
        HandleMouseInput();
        HandleTouchInput();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            RotateAroundTarget(delta.x, delta.y);
            lastMousePosition = Input.mousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            ZoomAtCursor(scroll * zoomSpeedMouse);
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
                RotateAroundTarget(touch.deltaPosition.x, touch.deltaPosition.y);
        }
        else if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float prevDist = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
            float currDist = (t0.position - t1.position).magnitude;
            float delta = currDist - prevDist;

            ZoomAtCenter(delta * zoomSpeedTouch);
        }
    }

    void RotateAroundTarget(float horizontal, float vertical)
    {
        Vector3 angles = new Vector3(-vertical, horizontal, 0f) * rotationSpeed;
        transform.RotateAround(target.position, transform.right, angles.x);
        transform.RotateAround(target.position, Vector3.up, angles.y);
    }

    void ZoomAtCursor(float zoomDelta)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 direction = (ray.GetPoint(currentDistance) - transform.position).normalized;

        currentDistance -= zoomDelta;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        transform.position += direction * zoomDelta;
        transform.position = target.position + (transform.position - target.position).normalized * currentDistance;
    }

    void ZoomAtCenter(float zoomDelta)
    {
        currentDistance -= zoomDelta;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        Vector3 direction = (transform.position - target.position).normalized;
        transform.position = target.position + direction * currentDistance;
    }
}

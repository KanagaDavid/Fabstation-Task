using UnityEngine;
using UnityEngine.SceneManagement;

public class DynamicPivot : MonoBehaviour
{
    [SerializeField] private GameObject pivotMarkerPrefab;
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private LayerMask modelLayer;

    private GameObject pivotMarker;
    private Vector3 pivotPoint;
    private bool isRotating = false;
    private Vector3 previousInputPosition;

    private void Start()
    {
        pivotPoint = transform.position;
        CreateOrMovePivotMarker(pivotPoint);
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        Vector3 inputPos = GetInputPosition();

        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            TrySetPivot(inputPos);
        }
        else if (Input.GetMouseButtonDown(1)) // Right-click
        {
            isRotating = true;
            previousInputPosition = inputPos;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        if (isRotating)
        {
            RotateModel(inputPos);
        }
    }

    private Vector3 GetInputPosition()
    {
        return Input.touchCount > 0 ? Input.GetTouch(0).position : Input.mousePosition;
    }

    private void TrySetPivot(Vector3 inputPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(inputPos);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, modelLayer))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                pivotPoint = hit.point;
                CreateOrMovePivotMarker(pivotPoint);
            }
        }
    }

    private void CreateOrMovePivotMarker(Vector3 position)
    {
        if (pivotMarker == null)
        {
            pivotMarker = Instantiate(pivotMarkerPrefab, position, Quaternion.identity);
        }
        else
        {
            pivotMarker.transform.position = position;
        }
    }

    private void RotateModel(Vector3 currentInputPosition)
    {
        Vector3 inputDelta = currentInputPosition - previousInputPosition;
        previousInputPosition = currentInputPosition;

        float rotX = -inputDelta.y * rotationSpeed;
        float rotY = inputDelta.x * rotationSpeed;

        if (Input.touchCount > 0)
        {
            rotX *= (100f / Screen.height);
            rotY *= (100f / Screen.width);
        }

        transform.RotateAround(pivotPoint, Vector3.up, rotY);
        transform.RotateAround(pivotPoint, transform.right, rotX);
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

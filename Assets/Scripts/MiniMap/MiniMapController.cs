using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MiniMapController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera miniMapCamera;
    [SerializeField] private RawImage miniMapDisplay;
    [SerializeField] private GameObject targetModel;
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private bool useSmoothTransition = true;
    [SerializeField] private float mainCameraHeight = 2f;
    [SerializeField] private float distanceFromModel = 3f;

    private Bounds modelBounds;
    private Vector3 targetPosition;
    private bool isTransitioning;
    private Quaternion originalRotation;

    private void Start()
    {
        SetupMiniMap();
        CalculateModelBounds();
        targetPosition = mainCamera.transform.position;
        originalRotation = mainCamera.transform.rotation;
    }

    private void Update()
    {
        if (isTransitioning && useSmoothTransition)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, transitionSpeed * Time.deltaTime);
            mainCamera.transform.rotation = originalRotation;

            if (Vector3.Distance(mainCamera.transform.position, targetPosition) < 0.01f)
                isTransitioning = false;
        }
    }

    private void SetupMiniMap()
    {
        miniMapCamera.orthographic = true;
        miniMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        var rt = new RenderTexture(256, 256, 16);
        miniMapCamera.targetTexture = rt;
        miniMapDisplay.texture = rt;
    }

    private void CalculateModelBounds()
    {
        var renderers = targetModel.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        modelBounds = renderers[0].bounds;
        foreach (var rend in renderers)
            modelBounds.Encapsulate(rend.bounds);

        AdjustMiniMapCamera();
    }

    private void AdjustMiniMapCamera()
    {
        Vector3 center = modelBounds.center;
        float maxDimension = Mathf.Max(modelBounds.size.x, modelBounds.size.z);

        miniMapCamera.transform.position = center + Vector3.up * (modelBounds.size.y + 10f);
        miniMapCamera.orthographicSize = maxDimension * 0.6f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(miniMapDisplay.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            Rect rect = miniMapDisplay.rectTransform.rect;
            Vector2 normalizedPoint = new(
                Mathf.Clamp01((localPoint.x - rect.x) / rect.width),
                Mathf.Clamp01((localPoint.y - rect.y) / rect.height)
            );

            MoveMainCamera(ConvertMiniMapPointToWorld(normalizedPoint));
        }
    }

    private Vector3 ConvertMiniMapPointToWorld(Vector2 normalizedPoint)
    {
        return new Vector3(
            Mathf.Lerp(modelBounds.min.x, modelBounds.max.x, normalizedPoint.x),
            0,
            Mathf.Lerp(modelBounds.min.z, modelBounds.max.z, normalizedPoint.y)
        );
    }

    private void MoveMainCamera(Vector3 targetPoint)
    {
        float cameraY = modelBounds.center.y + mainCameraHeight;

        Vector3 newPos = new(
            Mathf.Clamp(targetPoint.x, modelBounds.min.x - 2, modelBounds.max.x + 2),
            cameraY,
            Mathf.Clamp(targetPoint.z - distanceFromModel, modelBounds.min.z - distanceFromModel, modelBounds.max.z + 2)
        );

        if (useSmoothTransition)
        {
            targetPosition = newPos;
            isTransitioning = true;
        }
        else
        {
            mainCamera.transform.position = newPos;
            mainCamera.transform.rotation = originalRotation;
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && targetModel != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(modelBounds.center, modelBounds.size);
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

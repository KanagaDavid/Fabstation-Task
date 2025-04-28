using UnityEngine;
using System;

public class SpherePoint : MonoBehaviour
{
    public static bool IsDragging = false;  // Shared flag

    private Vector3 offset;
    public Action onDragEnd;

    void OnMouseDown()
    {
        offset = transform.position - GetMouseWorldPos();
        IsDragging = true;
    }

    void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + offset;
    }

    void OnMouseUp()
    {
        IsDragging = false;
        onDragEnd?.Invoke();
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}

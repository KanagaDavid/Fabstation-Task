using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Pointer : MonoBehaviour
{
    public GameObject spherePrefab;
    public Material lineMaterial;

    List<GameObject> allSpheres = new List<GameObject>();
    List<LineRenderer[]> lineGroups = new List<LineRenderer[]>();
    float lineWidth = 0.03f;
    List<GameObject> arcObjects = new List<GameObject>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (SpherePoint.IsDragging) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject obj = Instantiate(spherePrefab, hit.point, Quaternion.identity);
                allSpheres.Add(obj);

                SpherePoint sp = obj.AddComponent<SpherePoint>();
                int groupIndex = (allSpheres.Count - 1) / 3;
                sp.onDragEnd = () => RecalculateAngleAndLines(groupIndex);

                if (allSpheres.Count % 3 == 0)
                {
                    int i = allSpheres.Count - 3;
                    Vector3 a = allSpheres[i].transform.position;
                    Vector3 b = allSpheres[i + 1].transform.position;
                    Vector3 c = allSpheres[i + 2].transform.position;

                    float angle = CalculateAngle(a, b, c);
                    Debug.Log($"Angle at B (click): {angle:F2}°");

                    //DrawArc(a, b, c, 0.2f);
                    arcObjects.Add(CreateAndDrawArc(a, b, c, 0.2f));

                    LineRenderer[] lines = new LineRenderer[2];
                    lines[0] = DrawLine(a, b);
                    lines[1] = DrawLine(c, b);
                    lineGroups.Add(lines);
                }
            }
        }
    }

    void RecalculateAngleAndLines(int groupIndex)
    {
        int start = groupIndex * 3;
        if (allSpheres.Count < start + 3) return;

        Vector3 a = allSpheres[start].transform.position;
        Vector3 b = allSpheres[start + 1].transform.position;
        Vector3 c = allSpheres[start + 2].transform.position;

        float angle = CalculateAngle(a, b, c);
        Debug.Log($"Angle at B (drag): {angle:F2}°");

        if (groupIndex < lineGroups.Count)
        {
            lineGroups[groupIndex][0].SetPosition(0, a);
            lineGroups[groupIndex][0].SetPosition(1, b);
            lineGroups[groupIndex][1].SetPosition(0, c);
            lineGroups[groupIndex][1].SetPosition(1, b);
        }

        if (groupIndex < arcObjects.Count)
        {
            Destroy(arcObjects[groupIndex]);
            arcObjects[groupIndex] = CreateAndDrawArc(a, b, c, 0.2f);
        }


       // DrawArc(a, b, c, 0.2f);
    }

    float CalculateAngle(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ba = a - b;
        Vector3 bc = c - b;
        return Vector3.Angle(ba, bc);
    }

    LineRenderer DrawLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Line");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material = lineMaterial;
        lr.widthMultiplier = lineWidth;
        return lr;
    }

    GameObject CreateAndDrawArc(Vector3 a, Vector3 b, Vector3 c, float radius)
    {
        GameObject arcObj = CreateArcObject();
        MeshFilter mf = arcObj.GetComponent<MeshFilter>();

        Vector3 dirAB = (a - b).normalized;
        Vector3 dirCB = (c - b).normalized;
        float angle = Vector3.Angle(dirAB, dirCB);

        CreateArcMesh(mf, b, dirAB, dirCB, radius, angle);

        // Increase Y position of the arc slightly
        arcObj.transform.position += new Vector3(0, 0.01f, 0);

        // Create TextMeshPro for angle display
        CreateAngleText(b + new Vector3(0, 0.2f, 0), angle, arcObj.transform);

        return arcObj;
    }



    GameObject CreateArcObject()
    {
        GameObject newArcObject = new GameObject("AngleArc");
        MeshFilter mf = newArcObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = newArcObject.AddComponent<MeshRenderer>();

        renderer.material = new Material(Shader.Find("Unlit/Color"));
        renderer.material.color = new Color(0f, 0f, 1f, 1f);
        renderer.material.renderQueue = 4000;

        return newArcObject;
    }

    void CreateArcMesh(MeshFilter meshFilter, Vector3 center, Vector3 startDir, Vector3 endDir, float radius, float angle)
    {
        int segments = Mathf.Max(4, Mathf.FloorToInt(angle / 10));

        Mesh mesh = new Mesh();

        Vector3 normal = Vector3.Cross(startDir, endDir).normalized;
        if (normal.magnitude < 0.001f)
        {
            normal = Vector3.Cross(startDir, Mathf.Abs(Vector3.Dot(startDir, Vector3.up)) < 0.9f ? Vector3.up : Vector3.forward).normalized;
        }

        Vector3[] vertices = new Vector3[segments + 2];
        vertices[0] = Vector3.zero;

        Vector3 localX = startDir.normalized;
        Vector3 localY = Vector3.Cross(normal, localX).normalized;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angleRad = t * angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(angleRad) * radius;
            float y = Mathf.Sin(angleRad) * radius;
            vertices[i + 1] = localX * x + localY * y;
        }

        int[] triangles = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshFilter.transform.position = center;
    }

    void CreateAngleText(Vector3 position, float angle, Transform parent)
    {
        GameObject textObj = new GameObject("AngleText");
        textObj.transform.SetParent(parent);
        textObj.transform.position = position;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = $"{angle:F1}°";  // 1 decimal place
        tmp.fontSize = 1.5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.red;
        tmp.sortingOrder = 10; // Make sure it renders on top
    }


    void DrawArc(Vector3 a, Vector3 b, Vector3 c, float radius)
    {
        GameObject arcObj = CreateArcObject();
        MeshFilter mf = arcObj.GetComponent<MeshFilter>();

        Vector3 dirAB = (a - b).normalized;
        Vector3 dirCB = (c - b).normalized;
        float angle = Vector3.Angle(dirAB, dirCB);

        CreateArcMesh(mf, b, dirAB, dirCB, radius, angle);

        arcObjects.Add(arcObj);
    }

    public void ResetEverything()
    {
        // Disable and clear all spheres
        foreach (var sphere in allSpheres)
        {
            if (sphere != null)
            {
                sphere.SetActive(false);
            }
        }
        allSpheres.Clear();

        // Disable and clear all line renderers
        foreach (var lineGroup in lineGroups)
        {
            foreach (var line in lineGroup)
            {
                if (line != null)
                {
                    line.gameObject.SetActive(false);
                }
            }
        }
        lineGroups.Clear();

        // Disable and clear all arc objects (with angle texts inside)
        foreach (var arc in arcObjects)
        {
            if (arc != null)
            {
                arc.SetActive(false);
            }
        }
        arcObjects.Clear();
    }

}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class PointSpawner : MonoBehaviour
{
    [SerializeField] Transform parent;
    [SerializeField] GameObject pointPrefab;
    [SerializeField] Vector3 spawnLocation;
    [SerializeField] LineRenderer lineRenderer;
    private List<Vector3> linePositions = new List<Vector3>();
    private List<Segment> segments = new List<Segment>();

    public void Clicked()
    {
        CreatePoint();
    }

    public Point CreatePoint()
    {
        GameObject newPoint = Instantiate(pointPrefab, parent);
        newPoint.transform.position += new Vector3 (0, 0, 2.5f);
        newPoint.GetComponent<Point>().SetPointSpawner(this);
        newPoint.GetComponent<Point>().TheTrashManComesOut();
        return newPoint.GetComponent<Point>();
    }

    public void AddPoint(Vector3 position)
    {
        if (linePositions.Count != 0)
        {
            segments.Add(new Segment (linePositions[linePositions.Count - 1], position));
        }

        if (Intersection(position))
        {
            FindObjectOfType<Finish>().FinishDrawing();
            return;
        }

        Vector3 newLinePosition = new Vector3 (position.x, position.y, position.z - 0.1f);
        position = new Vector3 (position.x, position.y, 10);
        PathNodeCreator.Instance.AddPathNode(new PathNode(position));
        linePositions.Add(newLinePosition);
        UpdateLine();
    }

    private bool Intersection(Vector3 position)
    {
        if (linePositions.Count == 0)
        {
            return false;
        }

        Segment checkSegment = new Segment(linePositions[linePositions.Count - 1], position);

        for (int i = 0; i < segments.Count; i++)
        {
            if (checkSegment.IsIntersection(segments[i]))
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateLine()
    {
        lineRenderer.positionCount = linePositions.Count;

        for (int i = 0; i < linePositions.Count; i++)
        {
            lineRenderer.SetPosition(i, linePositions[i]);
        }
    }

    public void DestroyLineRenderer()
    {
        Destroy(lineRenderer.gameObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHelper : MonoBehaviour
{
    [SerializeField] Vector2[] debugPositionArray;
    [SerializeField] PointSpawner pointSpawner;
    [SerializeField] PathNodeCreator pathNodeCreator;
    [SerializeField] bool usingThis;

    public void SetUpPoints()
    {
        Debug.Log("hi");
        if (!usingThis) { return; }

        foreach(Vector2 position in debugPositionArray)
        {
            Vector3 actualPosition = new Vector3(position.x, position.y, 10f);
            Point newPoint = pointSpawner.CreatePoint();
            newPoint.SnapPoint(actualPosition);
        }

        pathNodeCreator.LetTheGamesBegin();
    }
}

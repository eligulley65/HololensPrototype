using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{
    //Reference to CutManager for events
    CutManager cutManager;
    [SerializeField] GameObject materialPrefab;

    private void Awake()
    {
        cutManager = FindObjectOfType<CutManager>();
    }

    private void Start() 
    {
        cutManager.OnInvalidShape += CutManager_OnInvalidShape;
    }

    private void CutManager_OnInvalidShape(object sender, EventArgs e)
    {
        DestroyAllPoints();
        FindObjectOfType<PointSpawner>().DestroyLineRenderer();
    }

    public void FinishDrawing()
    {
        ReadyForCut();
    }

    private void ReadyForCut()
    {
        DestroyAllPoints();
        FindObjectOfType<PointSpawner>().DestroyLineRenderer();
        cutManager.SetThingToCut(Instantiate(materialPrefab, transform.position, Quaternion.identity));
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        PathNodeCreator.Instance.LetTheGamesBegin();
    }

    private void DestroyAllPoints()
    {
        //Destroy all point objects
        Point[] points = FindObjectsOfType<Point>();
        foreach(Point point in points)
        {
            Destroy(point.gameObject);
        }
    }
}

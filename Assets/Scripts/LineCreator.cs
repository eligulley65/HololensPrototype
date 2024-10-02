/*
Created By Elijah Gulley

This script tracks the mousePosition and stores its position as it moves after
a short delay.The result is a list a Vector3 positions that approximate the mouse's
move path. These positions are then used to create a line object, which can then
be simplified to reduce complexity later on.

*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineCreator : MonoBehaviour
{
    //Creating a public instance like this ensures all objects have access to this object
    //without needing a reference or any static methods
    public static LineCreator Instance { get; private set; }

    //Serialized values that MUST be set in the inspector
    [SerializeField] GameObject lineRendererPrefab;

    //The position list that is used to create the line
    public List<Vector3> positionList = new List<Vector3>();
    //The line object created by this script
    LineRenderer lineRenderer;
    MeshCollider meshCollider;
    //Values to track when postions can be added
    //Keep the position add delay at least above 3 for performance
    const int POSITION_ADD_DELAY = 5; //measured in frames

    //Singleton pattern to ensure only the first version of this object ever exists.
    //If instance has a value, then another object of this type must have ran its Awake
    //method first. A Debug statement is sent out to find the problem, then this object is 
    //destroyed. If there is no other object, then this one becomes the Instance.
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Hey! There's an imposter LineCreator! Kill it with fire! " + transform.position + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    //Start method to create the line object and access its lineRenderer
    void Start()
    {
        lineRenderer = Instantiate(lineRendererPrefab, transform.position, Quaternion.identity).GetComponent<LineRenderer>();
        meshCollider = lineRenderer.gameObject.GetComponent<MeshCollider>();
    }

    public void DeleteLine()
    {
        lineRenderer.enabled = false;
    }

    //Get method for the LineRenderer. Should never be set by other objects
    public LineRenderer GetLineRenderer()
    {
        return lineRenderer;
    }
}
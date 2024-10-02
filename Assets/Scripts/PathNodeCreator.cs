/*
Created by Elijah Gulley

Takes a line given to it from a LineCreator object and creates PathNodes at each point
on the line. Then, through an event, this is sent to the follower assigned to this object.
Assigning objects this way is done through Serialized values in the Unity inspector.

*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PathNodeCreator : MonoBehaviour
{
    //Creating a public instance like this ensures all objects have access to this object
    //without needing a reference or any static methods
    public static PathNodeCreator Instance { get; private set; }

    //An event that is invoked when all the PathNodes have been created
    //This event is listened to by all followers
    public event EventHandler OnPathNodeCreationFinished;

    //The PathNode list that eventually gets used by the Follower object
    private List<PathNode> pathNodeList = new List<PathNode>();

    //CutManager reference for events
    private CutManager cutManager;
 
    //Singleton pattern to ensure only the first version of this object ever exists.
    //If instance has a value, then another object of this type must have ran its Awake
    //method first. A Debug statement is sent out to find the problem, then this object is 
    //destroyed. If there is no other object, then this one becomes the Instance.
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Hey! There's an imposter PathNodeCreator! Kill it with fire! " + transform.position + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //find cutmanager
        cutManager = FindObjectOfType<CutManager>();
    }

    //Start method to listen to events
    private void Start() 
    {
        cutManager.OnInvalidShape += CutManager_OnInvalidShape;
    }

    private void CutManager_OnInvalidShape(object sender, EventArgs e)
    {
        pathNodeList.Clear();
    }
    
    public void LetTheGamesBegin()
    {
        pathNodeList.Add(pathNodeList[pathNodeList.Count - 1]);
        MakeClockwise();
        OnPathNodeCreationFinished?.Invoke(this, EventArgs.Empty);
    }

    //Runs through each position in the line and created a PathNode there
    //Then, adds each PathNode to the list
    public void CreatePathNodes(LineRenderer lineRenderer)
    {
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            PathNode pathNode = new PathNode(lineRenderer.GetPosition(i));
            pathNodeList.Add(pathNode);
        }
    }

    //Uses the maxes and mins of the object to find its winding order
    //If the order is counter-clockwise, it gets reversed
    private void MakeClockwise()
    {
        //Variables to store the max and min values and indices
        Vector3 maxX = new Vector3 (-Mathf.Infinity, 0, 0);
        Vector3 minX = new Vector3 (Mathf.Infinity, 0, 0); 
        Vector3 maxY = new Vector3 (0, -Mathf.Infinity, 0);
        Vector3 minY = new Vector3 (0, Mathf.Infinity, 0);

        int maxXindex = 0;
        int minXindex = 0;
        int maxYindex = 0;
        int minYindex = 0;

        //Find each max and min
        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].GetWorldPosition().x > maxX.x)
            {
                maxX = pathNodeList[i].GetWorldPosition();
                maxXindex = i;
            }
            if (pathNodeList[i].GetWorldPosition().y > maxY.y)
            {
                maxY = pathNodeList[i].GetWorldPosition();
                maxYindex = i;
            }
            if (pathNodeList[i].GetWorldPosition().x < minX.x)
            {
                minX = pathNodeList[i].GetWorldPosition();
                minXindex = i;
            }
            if (pathNodeList[i].GetWorldPosition().y < minY.y)
            {
                minY = pathNodeList[i].GetWorldPosition();
                minYindex = i;
            }
        }

        //check each max and min point for its winding order
        //all should have the same winding order
        if (maxX.y < GetItem(pathNodeList, maxXindex + 1).GetWorldPosition().y && minX.y > GetItem(pathNodeList, minXindex + 1).GetWorldPosition().y 
            && maxY.x > GetItem(pathNodeList, maxYindex + 1).GetWorldPosition().x && minY.x < GetItem(pathNodeList, minYindex + 1).GetWorldPosition().x)
        {
            //the winding order is counter-clockwise, so reverse it
            pathNodeList.Reverse();
        }
    }

    //Custom get item method to handle outside of bounds exceptions
    //by looping back through the list
    private T GetItem<T>(List<T> list, int index)
    {
        if (list.Count <= 0)
        {
            return list[0];
        }

        if (index >= list.Count)
        {
            return list[index % list.Count];
        }
        if (index < 0)
        {
            return list[index % list.Count + list.Count];
        }

        return list[index];
    }

    public void AddPathNode(Vector3 newLocation)
    {
        pathNodeList.Add(new PathNode(newLocation));
    }

    public void AddPathNode(PathNode pathNode)
    {
        pathNodeList.Add(pathNode);
    }

    //Get method for the pathNodeList. pathNodeList should never be set by other scripts
    public List<PathNode> GetPathNodeList()
    {
        return pathNodeList;
    }
}
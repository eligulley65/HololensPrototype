using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    private LineRenderer debugLines;
    private PointSpawner pointSpawner;
    private bool theTrashMan = false;

    private void Start() 
    {
        //Search for the DebugLines object out of all LineRenderers in the scene
        //and store the reference in debugLines
        foreach (LineRenderer thingy in FindObjectsOfType<LineRenderer>())
        {
            if (thingy.gameObject.CompareTag("DebugLines"))
            {
                debugLines = thingy;
                debugLines.enabled = false;
                return;
            }
        }

        if (!debugLines)
        {
            Debug.Log("Theres no debuglines object stupid head");
        }
    }

    //Public method to set the point spawner attribute
    public void SetPointSpawner(PointSpawner newPointSpawner)
    {
        pointSpawner = newPointSpawner;
    }

    //Called when the point is done moving to snap it in place on the
    //surface of the object
    public void SnapPoint()
    {
        //Create a raycast and record what got hit in hitInfo
        Physics.Raycast(transform.position, new Vector3 (0, 0, 1), out RaycastHit hitInfo);
        //If the object hit is the thing to cut, set the position to the impact point
        if (hitInfo.collider.gameObject.CompareTag("ThingToCut"))
        {
            transform.position = hitInfo.point;
            Debug.Log(transform.position);
        }
        //disable interactability and load the point into point spawner
        theTrashMan = false;
        debugLines.enabled = false;
        pointSpawner.AddPoint(transform.position);
        CantTouchThis();
    }

    //An overloaded version of SnapPoint to instead snap the point to 
    //a given point, rather than using raycasts. Used for debugging
    public void SnapPoint(Vector3 positionToSnapTo)
    {
        transform.position = positionToSnapTo;
        pointSpawner.AddPoint(transform.position);
        CantTouchThis();
    }

    //Disables click and drag functionality once the point is in place
    private void CantTouchThis()
    {
        GetComponent<Collider>().enabled = false;
    }

    //set the trash man
    public void TheTrashManComesOut()
    {
        theTrashMan = true;
    }

    private void Update() 
    {
        //only run the rest of the code if theres a trash man
        if (!theTrashMan) { return; }

        //handle and display the debuglines for this point
        debugLines.enabled = true;

        Physics.Raycast(transform.position, new Vector3 (0, 0, 1), out RaycastHit hitInfo);

        if (hitInfo.Equals(null))
        {
            Debug.Log("Im not touching you");
        }

        if (hitInfo.collider.gameObject.CompareTag("ThingToCut"))
        {
            Vector3[] stuff = new Vector3[] {transform.position, hitInfo.point};
            debugLines.SetPositions(stuff);
        }
    }
}

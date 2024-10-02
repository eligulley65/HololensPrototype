/* Created by Elijah Gulley

This script handles the necessary calculations in order to complete the outside cut
on the object once the line is fully drawn.

Uses data from CutManager to determine the size of the object, then 
uses the corners of the faces as well as the maximum and minimum points
in the x and y directions to triangulate the front face. The face is then
copied to the back in reverse.

The sides are then added using a predetermined sequence.

*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;

[RequireComponent (typeof (CutManager))]
public class OutsideCutManager : MonoBehaviour
{
    //First, identify corners
    //Find the maximums and minimums of all the non-corner points
        //Create triangles with those points using their closest two corners
    //Identify all interior points.
        //Using cross products, find which points are ears
        //This will use the same checks as the original ear clipping algorithm,
        //however we want the points that would FAIL these checks
    //Once all interior points are found, move through them and cut the ears, removing them from
    //a master list of all indices
    //Finally, create the triangles for each non-interior point by using one neighbor and the closest
    //corner for each point.
        //Make sure to only use the points that are DIRECTLY adjacent to avoid triangles that are too big

    //When adding triangles: go CLOCKWISE

    //Reference to the CutManager attached to this object
    CutManager cutManager;
    //Reference to the follower for event handling reasons
    Follower follower;
    //Reference to the object being cut and its relevant components
    GameObject thingToCut;
    MeshRenderer thingToCutRenderer;
    //Variables to store where the corners are
    int topRightCornerIndex, topLeftCornerIndex, bottomLeftCornerIndex, bottomRightCornerIndex;
    //Varables to store where the backside corners are
    int backTRCornerIndex, backTLCornerIndex, backBLCornerIndex, backBRCornerIndex;
    //List to store the vertices for just the first side
    readonly List<Vector3> frontSideVertices = new List<Vector3>();
    //list to store all vertices
    readonly List<Vector3> finalVertices = new List<Vector3>();
    //List to store the final triangle list after duplication
    readonly List<int> finalTriangles = new List<int>();
    //List to store the indices of different vertices within the frontSideVertices list
    readonly List<int> indices = new List<int>();
    //A list of indices that doesn't include the corners of the object
    //This is used to make trianglulation a bit easier
    readonly List<int> frontIndicesWOCorners = new List<int>();
    //List of indices in frontSideVertices representing triangles in the final mesh
    readonly List<int> triangles = new List<int>();
    //List of indices that represent interior ears
    readonly List<int> insidePoints = new List<int>();
    //Store max and min indices
    //Initialized each at a neg or pos million so finding the points is easier
    readonly float[] maxesAndMinVals = {-1000000, -1000000, 1000000, 1000000}; //xMax, yMax, xMin, yMin
    readonly int[] maxesAndMinsIndices = new int[4]; //xMax, yMax, xMin, yMin

    readonly List<Segment> lineSegments = new List<Segment>();
    [SerializeField] bool SmallBoiTime = false;
    [SerializeField] GameObject material;

    private void Start() 
    {
        //Initialize object references
        cutManager = gameObject.GetComponent<CutManager>();
        follower = cutManager.GetFollower();

        //Subscribe to events
        follower.OnPathCompleted += Follower_OnPathCompleted;
        cutManager.OnInsideCompleted += CutManager_OnInsideCompleted;
    }

    //An event that triggers when the CutManager script finishes its algorithm
    private void CutManager_OnInsideCompleted(object sender, EventArgs e)
    {
        //Gets a copied thingToCut for the outside part
        thingToCut = cutManager.GetThingToCutCopy();
        thingToCutRenderer = thingToCut.GetComponent<MeshRenderer>();

        //Run the algorithm
        FindFrontSideVertices();
        FindLineSegments();
        HandleMaxesAndMins();
        HandleInsidePoints();
        HandleOutsidePoints();
        AddOtherSide();
        HandleSides();
        DuplicateVertices();
        UpdateMesh();
        //thingToCut.transform.localScale = new Vector3(1, 1, 1);
        //thingToCut.GetComponent<ObjectManipulator>().enabled = true;
        //thingToCut.GetComponent<NearInteractionGrabbable>().enabled = true;
        //thingToCut.transform.position = new Vector3(thingToCut.transform.position.x, thingToCut.transform.position.y, 2f);

    }

    //Event that triggers when the follower finishes its path
    private void Follower_OnPathCompleted(object sender, EventArgs e)
    {
//        TransformVerticies();
        UpdateMesh();
        //Change the object's position to look nice
//        MoveAndScale moveAndScale = new MoveAndScale(cutManager.GetThingToCut(), thingToCut, material);
//        moveAndScale.MoveObjects();
        //These values will need to change based on the object's starting position
        thingToCut.transform.localScale = new Vector3(1, 1, 1);
        thingToCut.GetComponent<ObjectManipulator>().enabled = true;
        thingToCut.GetComponent<NearInteractionGrabbable>().enabled = true;
        thingToCut.transform.position = new Vector3(thingToCut.transform.position.x, thingToCut.transform.position.y, 2f);
    }

    //Uses the pathnodelist and the extents of the object to find all the vertices
    //on the front side. It also stores the indices of the corners for use later
    private void FindFrontSideVertices()
    {
        //Get the pathnodelist
        List<PathNode> pathNodeList = PathNodeCreator.Instance.GetPathNodeList();

        for (int i = 0; i < pathNodeList.Count; i++){
            Debug.Log("out manager " + pathNodeList[i].GetWorldPosition());
        }
        //Set up the frontsidevertices list as well as the indices list
        for (int i = 0; i < pathNodeList.Count - 1; i++)
        {
            frontSideVertices.Add(pathNodeList[i].GetWorldPosition());
            indices.Add(i);
        }

        //Initialize frontIndicesWOCorners
        frontIndicesWOCorners.AddRange(indices);

        float xExtents = thingToCutRenderer.bounds.extents.x;
        float yExtents = thingToCutRenderer.bounds.extents.y;

        //Use the extents to calculate the corners
        Vector3 topRightCorner = thingToCut.transform.position + new Vector3(xExtents, yExtents);
        Vector3 topLeftCorner = thingToCut.transform.position + new Vector3(-xExtents, yExtents);
        Vector3 bottomLeftCorner = thingToCut.transform.position + new Vector3(-xExtents, -yExtents);
        Vector3 bottomRightCorner = thingToCut.transform.position + new Vector3(xExtents, -yExtents);

        //Set the z values of the corners to be flush with the front
        topRightCorner.z = frontSideVertices[0].z;
        topLeftCorner.z = frontSideVertices[0].z;
        bottomLeftCorner.z = frontSideVertices[0].z;
        bottomRightCorner.z = frontSideVertices[0].z;

        //Add the corners to vertices and store their indices
        frontSideVertices.Add(topRightCorner);
        indices.Add(frontSideVertices.Count - 1);
        topRightCornerIndex = frontSideVertices.Count - 1;

        frontSideVertices.Add(topLeftCorner);
        indices.Add(frontSideVertices.Count - 1);
        topLeftCornerIndex = frontSideVertices.Count - 1;

        frontSideVertices.Add(bottomLeftCorner);
        indices.Add(frontSideVertices.Count - 1);
        bottomLeftCornerIndex = frontSideVertices.Count - 1;

        frontSideVertices.Add(bottomRightCorner);
        indices.Add(frontSideVertices.Count - 1);
        bottomRightCornerIndex = frontSideVertices.Count - 1;

        //Debug.Log("TR: " + topRightCorner + " BR: " + bottomRightCorner + " BL: " + bottomLeftCorner + " TL: " + topLeftCorner);
    }

    //Finds the maxes and mins of the inner points
    //Then creates the triangles associated with them
    private void HandleMaxesAndMins()
    {
        FindMaxesAndMins();
        CreateMaxAndMinTriangles();
    }

    //Find each point with the max and min x and y value
    private void FindMaxesAndMins()
    {
        //Loop through all vertices except for the corners and find the maxes and mins
        for (int i = 0; i < frontSideVertices.Count - 4; i++)
        {
            //For each point, check each max and min
            for (int j = 0; j < 4; j++)
            {
                switch (j)
                {
                    //check it against xMax
                    case 0:
                        if (frontSideVertices[i].x > maxesAndMinVals[j])
                        {
                            maxesAndMinVals[j] = frontSideVertices[i].x;
                            maxesAndMinsIndices[j] = i;
                        }
                        break;
                    //check it against yMax
                    case 1:
                        if (frontSideVertices[i].y > maxesAndMinVals[j])
                        {
                            maxesAndMinVals[j] = frontSideVertices[i].y;
                            maxesAndMinsIndices[j] = i;
                        }
                        break;
                    //check it against xMin
                    case 2:
                        if (frontSideVertices[i].x < maxesAndMinVals[j])
                        {
                            maxesAndMinVals[j] = frontSideVertices[i].x;
                            maxesAndMinsIndices[j] = i;
                        }
                        break;
                    //check it against yMin
                    case 3:
                        if (frontSideVertices[i].y < maxesAndMinVals[j])
                        {
                            maxesAndMinVals[j] = frontSideVertices[i].y;
                            maxesAndMinsIndices[j] = i;
                        }
                        break;
                }
            }
        }
    }

    //Creates four triangles, one for each max and min
    private void CreateMaxAndMinTriangles()
    {
        //Add a triangle to triangles for each max or min in CLOCKWISE order
        //Add the xMax triangle
        triangles.Add(maxesAndMinsIndices[0]);
        triangles.Add(topRightCornerIndex);
        triangles.Add(bottomRightCornerIndex);

        //Add the linesegments for the triangle
        lineSegments.Add(new Segment(frontSideVertices[maxesAndMinsIndices[0]], frontSideVertices[topRightCornerIndex]));
        lineSegments.Add(new Segment(frontSideVertices[maxesAndMinsIndices[0]], frontSideVertices[bottomRightCornerIndex]));

        //Add the yMax triangle
        triangles.Add(maxesAndMinsIndices[1]);
        triangles.Add(topLeftCornerIndex);
        triangles.Add(topRightCornerIndex);

        //Add the linesegments for the triangle
        lineSegments.Add(new Segment(frontSideVertices[maxesAndMinsIndices[1]], frontSideVertices[topLeftCornerIndex]));
        lineSegments.Add(new Segment(frontSideVertices[maxesAndMinsIndices[1]], frontSideVertices[topRightCornerIndex]));

        //Add the xMin triangle
        triangles.Add(maxesAndMinsIndices[2]);
        triangles.Add(bottomLeftCornerIndex);
        triangles.Add(topLeftCornerIndex);

        //Add the linesegments for the triangle
        lineSegments.Add(new Segment(frontSideVertices[maxesAndMinsIndices[2]], frontSideVertices[bottomLeftCornerIndex]));
        lineSegments.Add(new Segment(frontSideVertices[maxesAndMinsIndices[2]], frontSideVertices[topLeftCornerIndex]));

        //Add the yMin triangle
        triangles.Add(maxesAndMinsIndices[3]);
        triangles.Add(bottomRightCornerIndex);
        triangles.Add(bottomLeftCornerIndex);

        //Add the linesegments for the triangle
        lineSegments.Add(new Segment(frontSideVertices[maxesAndMinsIndices[3]], frontSideVertices[bottomRightCornerIndex]));
        lineSegments.Add(new Segment(frontSideVertices[maxesAndMinsIndices[3]], frontSideVertices[bottomLeftCornerIndex]));
    }

    //Handles points that jut inside the object
    private void HandleInsidePoints()
    {
        //a list with equal values to frontIndicesWOCorners, but we can
        //remove things from it without breaking anything
        List<int> tempFrontIndices = new List<int>();
        tempFrontIndices.AddRange(frontIndicesWOCorners);
        List<int> tempInsidePoints = new List<int>();

        FindInsidePoints(tempInsidePoints, frontIndicesWOCorners);

        int count = 0;

        while(tempInsidePoints.Count != 0)
        {
            count++;

            for (int i = 0; i < tempFrontIndices.Count; i++)
            {
                //draw the triangle for the inside point
                if (tempInsidePoints.Contains(tempFrontIndices[i]))
                {
                    int currentIndex = tempFrontIndices[i];
                    int prevIndex = GetItem(tempFrontIndices, i - 1);
                    int nextIndex = GetItem(tempFrontIndices, i + 1);

                    triangles.Add(currentIndex);
                    triangles.Add(prevIndex);
                    triangles.Add(nextIndex);

                    tempFrontIndices.RemoveAt(i);
                    //prevent overflow errors or elements being skipped
                    i--;
                }
            }

            tempInsidePoints.Clear();

            FindInsidePoints(tempInsidePoints, tempFrontIndices);

            //Remove from tempInsidePoints all inside points that aren't in tempFrontIndices
            for (int i = 0; i < tempInsidePoints.Count; i++)
            {
                if (!tempFrontIndices.Contains(tempInsidePoints[i]))
                {
                    tempInsidePoints.RemoveAt(i);
                    //subtract from i to make sure every element gets checked
                    i--;
                }
            }
        } 
    }

    //Finds all points that jut inside the object using orientation checks
    private void FindInsidePoints(List<int> ipList, List<int> totalListToCheck)
    {
        //Check if a triangle can be drawn with adjacent points
        //Essentially run the orientation calculation on each of the points
        for (int i = 0; i < totalListToCheck.Count; i++)
        {
            Vector3 current = frontSideVertices[totalListToCheck[i]];
            Vector3 prev = GetItem(frontSideVertices, GetItem(totalListToCheck, i - 1));
            Vector3 next = GetItem(frontSideVertices, GetItem(totalListToCheck, i + 1));

            //if the three points are in clockwise order
            if (Segment.Orientation(current, prev, next) == 1)
            {
                ipList.Add(totalListToCheck[i]);
                if (!insidePoints.Contains(totalListToCheck[i]))
                {
                    insidePoints.Add(totalListToCheck[i]);
                }
            }
        }
    }

    //Creates each triangle associated with each inner point by
    //connecting a point, the next point, and the closest corner
    //makes sure the current corner is valid by checking for intersections
    private void HandleOutsidePoints()
    {
        //Remove inside points
        List<int> frontIndicesWOInsides = new List<int>();
        
        //set up frontIndicesWOInsides
        for (int i = 0; i < frontIndicesWOCorners.Count; i++)
        {
            if (!insidePoints.Contains(i))
            {
                frontIndicesWOInsides.Add(i);
            }
        }

        //Loop through each vertex, not including corners or inside points
        for (int i = 0; i < frontIndicesWOInsides.Count; i++)
        {
            //Create temp Segments for use later
            Segment temp;
            Segment nextTemp;
            //Get the current vertex from front side vertices
            Vector3 current = GetItem(frontSideVertices, frontIndicesWOInsides[i]);
            //Get the next vertex, using GetItem to account for wrap around with frontIndicesWOCorners
            Vector3 next = GetItem(frontSideVertices, GetItem(frontIndicesWOInsides, i + 1));

            //Get the indices of the three for adding to triangles later
            int currentIndex = frontIndicesWOInsides[i];
            int nextIndex = GetItem(frontIndicesWOInsides, i + 1);

            int[] corners = {topRightCornerIndex, bottomRightCornerIndex, bottomLeftCornerIndex, topLeftCornerIndex};

            //loop through the four corners, also c# is better
            for (int c = 0; c < 4; c++)
            {
                //set temp equal to the line between the current vertex and the current corner
                temp = new Segment (current, frontSideVertices[corners[c]]);
                //set nextTemp equal to the line between the next vertex and the current corner
                nextTemp = new Segment (next, frontSideVertices[corners[c]]);

                bool intersection = false;

                //Loop through the set of Segments to find any intersections
                for (int e = 0; e < lineSegments.Count; e++)
                {
                    //if the line between current and the corner intersects with something
                    if (temp.IsIntersection(lineSegments[e]))
                    {
                        intersection = true;
                        break;
                    }
                    //if the line between next and the corner intersects with something
                    else if (nextTemp.IsIntersection(lineSegments[e]))
                    {
                        intersection = true;
                        break;
                    }
                }

                if (!intersection)
                {
                    //Add the triangle and break
                    triangles.Add(currentIndex);
                    triangles.Add(corners[c]);
                    triangles.Add(nextIndex);

                    lineSegments.Add(temp);
                    lineSegments.Add(nextTemp);
                    break;
                }
            }
        }
    }

    //Creates an array of all line segments between each point on the inside shape
    private void FindLineSegments()
    {
        for (int i = 0; i < frontIndicesWOCorners.Count; i++)
        {
            //Get the current vertex from front side vertices
            Vector3 current = GetItem(frontSideVertices, frontIndicesWOCorners[i]);
            //Get the next vertex, using GetItem to account for wrap around with frontIndicesWOCorners
            Vector3 next = GetItem(frontSideVertices, GetItem(frontIndicesWOCorners, i + 1));

            lineSegments.Add(new Segment(current, next));
        }
    }

    //Takes the triangles from the front face and flips them for the back face
    //The width of the object is also taken into account
    private void AddOtherSide()
    {
        //The offset helps determine what the indices actually are
        int indexOffset = frontSideVertices.Count;
        //Temp variables to avoid infinite loops as objects are added to the lists
        int vertexLimit = frontSideVertices.Count;
        int triangleLimit = triangles.Count;

        //Initialize finalVertices with the values from frontSideVertices
        finalVertices.AddRange(frontSideVertices);

        //run through the vertices
        for (int i = 0; i < vertexLimit; i++)
        {
            //Add the back side vertex using the object's width
            finalVertices.Add(frontSideVertices[i] + new Vector3(0, 0, cutManager.GetWidth()));
        }

        //Add the new indices to triangles, but backwards
        for (int i = triangleLimit - 1; i >= 0; i--)
        {
            triangles.Add(triangles[i] + indexOffset);
        }

        //Store the indices of the back corners
        backTRCornerIndex = topRightCornerIndex + indexOffset;
        backTLCornerIndex = topLeftCornerIndex + indexOffset;
        backBLCornerIndex = bottomLeftCornerIndex + indexOffset;
        backBRCornerIndex = bottomRightCornerIndex + indexOffset;
    }

    //Creates the triangles necessary for the sides of the object
    //Both the inner sides as well as the outer ones
    private void HandleSides()
    {
        //Handle the inner sides
        //This code is essentially identical to the code in CutManager,
        //except the sides are flipped to face inwards instead
        for (int i = 0; i < frontSideVertices.Count - 4; i++)
        {
            int current = i;
            int next = GetItem<int>(frontIndicesWOCorners, current + 1);
            int backCurrent = i + frontSideVertices.Count;
            int backNext = next + frontSideVertices.Count;

            triangles.Add(backNext);
            triangles.Add(current);
            triangles.Add(next);

            triangles.Add(current);
            triangles.Add(backNext);
            triangles.Add(backCurrent);
        }

        //Do the outer sides 
        //DO NOT CHANGE THESE ORDERS
        //right side
        triangles.Add(topRightCornerIndex);
        triangles.Add(backTRCornerIndex);
        triangles.Add(bottomRightCornerIndex);

        triangles.Add(bottomRightCornerIndex);
        triangles.Add(backTRCornerIndex);
        triangles.Add(backBRCornerIndex);

        //top side
        triangles.Add(topRightCornerIndex);
        triangles.Add(topLeftCornerIndex);
        triangles.Add(backTLCornerIndex);

        triangles.Add(backTLCornerIndex);
        triangles.Add(backTRCornerIndex);
        triangles.Add(topRightCornerIndex);

        //left side
        triangles.Add(topLeftCornerIndex);
        triangles.Add(bottomLeftCornerIndex);
        triangles.Add(backTLCornerIndex);

        triangles.Add(backTLCornerIndex);
        triangles.Add(bottomLeftCornerIndex);
        triangles.Add(backBLCornerIndex);

        //Bottom side
        triangles.Add(bottomLeftCornerIndex);
        triangles.Add(bottomRightCornerIndex);
        triangles.Add(backBRCornerIndex);

        triangles.Add(backBRCornerIndex);
        triangles.Add(backBLCornerIndex);
        triangles.Add(bottomLeftCornerIndex);
    }

    //creates a new vertex for each triangle index, allowing the final object to
    //appear more cohesive. This is based on how Unity applies lighting over complex shapes
    private void DuplicateVertices()
    {
        List<Vector3> tempFinalVertices = new List<Vector3>();
        tempFinalVertices.AddRange(finalVertices);
        finalVertices.Clear();

        for (int i = 0; i < triangles.Count; i++)
        {
            finalVertices.Add(tempFinalVertices[triangles[i]]);
            finalTriangles.Add(i);
        }
    }

    private void TransformVerticies(){
        for (int i = 0; i < finalVertices.Count; i++){
//            finalVertices[i] = calc.CalculateNewPosition(finalVertices[i]);
        }
    }

    //Create the final mesh
    private void UpdateMesh()
    {
        MeshCollider meshCollider = thingToCut.GetComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        MeshFilter thingToCutFilter = thingToCut.GetComponent<MeshFilter>();

        if (SmallBoiTime)
        {
            //ConvertToSmallBoiUnits();
        }

        mesh.Clear();
        mesh.vertices = finalVertices.ToArray();
        mesh.triangles = finalTriangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
        thingToCutFilter.mesh = mesh;
    }

    private void ConvertToSmallBoiUnits()
    {
        for (int i = 0; i < finalVertices.Count; i++)
        {
            finalVertices[i] *= 0.1f;
        }
    }

    //Utility Methods

    //Custom get item method to handle outside of bounds exceptions
    //by looping back through the list
    private T GetItem<T>(List<T> list, int index)
    {
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

    //Useful method for debugging
    public static void PrintList<T> (List<T> list, String addition = "")
    {
        String fullPoop = "";
        foreach(T thing in list)
        {
            fullPoop += thing + ", ";
        }
        Debug.Log(fullPoop + " " + addition);
    }
}
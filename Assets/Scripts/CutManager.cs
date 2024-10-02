/* Created by Elijah Gulley

This script handles the necessary calculations in order to complete the cut
on the object once the line is fully drawn.

Uses a triangulation algorithm for the front face, then copies the triangles
to the back face, just reversed.

Then adds the sides using a predetermined sequences of triangles

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.Rendering;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using System.CodeDom;

public class CutManager : MonoBehaviour
{
    public event EventHandler OnInsideCompleted;
    public event EventHandler OnInvalidShape;

    [SerializeField] GameObject thingToCut;
    [SerializeField] Transform thingToCutParent;
    GameObject thingToCutCopy;
    [SerializeField] Follower follower;
    [SerializeField] bool SmallBoiTime = false;
    MeshRenderer thingToCutRenderer;
    MeshFilter thingToCutFilter;
    Bounds thingToCutBounds;
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> frontVertices = new List<Vector3>();
    List<Vector3> finalVertices = new List<Vector3>();
    List<int> indices = new List<int>();
    List<int> triangles = new List<int>();
    List<int> finalTriangles = new List<int>();
    float width;
    Mesh mesh;

    [SerializeField] GameObject material;

    private void Start()
    {
        //Subscribe to the events from PathNodeCreator and Follower
        PathNodeCreator.Instance.OnPathNodeCreationFinished += PathNodeCreator_OnPathNodeCreationFinished;
        follower.OnPathCompleted += Follower_OnPathCompleted;

        //Initialize the references from the material to cut
        thingToCutRenderer = thingToCut.GetComponent<MeshRenderer>();
        thingToCutBounds = thingToCutRenderer.bounds;
        thingToCutFilter = thingToCut.GetComponent<MeshFilter>();

        //Store the width of the object
        width = thingToCutBounds.extents.z * 2;

        //Initialize the mesh
        mesh = new Mesh();
    }

    private void PathNodeCreator_OnPathNodeCreationFinished(object sender, EventArgs e)
    {
        //Create inside cut
        FindVertices();
        
        //if the shape is invalid reset
        if (!IsShapeValid())
        {
            ClearLists();
            OnInvalidShape?.Invoke(this, EventArgs.Empty);
            return;
        } 

        CheckClockwise();
        Triangulate();
        AddOtherSide();
        AddSides();
        DuplicateVertices();
        thingToCutCopy = Instantiate(thingToCut, thingToCut.transform.position, Quaternion.identity, thingToCutParent);
        OnInsideCompleted?.Invoke(this, EventArgs.Empty);
        CheckForMeshErrors();
//        TransformVerticies();
        UpdateMesh();
        thingToCut.transform.localScale = new Vector3(1, 1, 1);
        thingToCut.GetComponent<ObjectManipulator>().enabled = true;
    }

    //Runs once the follower finishes its path
    private void Follower_OnPathCompleted(object sender, EventArgs e)
    {
        CheckForMeshErrors();
//        TransformVerticies();
        UpdateMesh();
        thingToCut.transform.localScale = new Vector3(1, 1, 1);
        thingToCut.GetComponent<ObjectManipulator>().enabled = true;
        thingToCut.GetComponent<NearInteractionGrabbable>().enabled = true;
        //thingToCut.transform.position = new Vector3 (thingToCut.transform.position.x, thingToCut.transform.position.y, 0f);
    }
    
    //Sets up the vertices lists and indices list with the data from the path nodes
    private void FindVertices()
    {
        List<PathNode> pathNodeList = PathNodeCreator.Instance.GetPathNodeList();
        for (int i = 0; i < pathNodeList.Count; i++){
            Debug.Log("Cut manager " + pathNodeList[i].GetWorldPosition());
        }
        for (int i = 0; i < pathNodeList.Count - 1; i++)
        {
            vertices.Add(pathNodeList[i].GetWorldPosition());
            frontVertices.Add(pathNodeList[i].GetWorldPosition());
            indices.Add(i);
        }
    }

    private void ClearLists()
    {
        vertices.Clear();
        frontVertices.Clear();
        indices.Clear();
    }

    private bool IsShapeValid()
    {
        //set up segment list
        List<Segment> segments = new List<Segment>();

        for (int i = 0; i < frontVertices.Count - 1; i++)
        {
            segments.Add(new Segment(frontVertices[i], frontVertices[i+1]));
        }
        //Add the last segment to loop it around
        segments.Add(new Segment(frontVertices[frontVertices.Count - 1], frontVertices[0]));

        //Run through segment list to find intersections
        for (int x = 0; x < segments.Count - 1; x++)
        {
            for (int y = x + 1; y < segments.Count; y++)
            {
                if (segments[x].IsIntersection(segments[y]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    //Checks if the vertices are in clockwise order, and reverses them if not
    private void CheckClockwise()
    {
        float sum = 0;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 current = vertices[i];
            Vector3 next = GetItem<Vector3>(vertices, i + 1);

            sum += (next.x - current.x)*(next.y + current.y);
        }

        if (sum < 0)
        {
            vertices.Reverse();
        }
    }

    //Method to test alternate algorithm ideas, w/o needing to get rid of the old one
    private void Triangulate()
    {
        int count = 0;

        //while there are more than 3 points left, 100 is the cap to stop infinite loops
        while(indices.Count > 3 && count < 100)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                if (indices.Count <= 3)
                {
                    break;
                }
    
                //stores whether or not a triangle is valid
                bool valid = true;
    
                //The current, next, and prev vertices and indices
                int currentIndex = indices[i];
                int nextIndex = GetItem(indices, i + 1);
                int prevIndex = GetItem(indices, i - 1);
                
                Vector2 current = vertices[currentIndex];
                Vector2 next = vertices[nextIndex];
                Vector2 prev = vertices[prevIndex];

                //if the orientation of the triangle is counterclockwise
                if (Segment.Orientation(prev, current, next) != 1)
                {
                    //no need to check the other points
                    continue;
                }

                //The area of the current triangle
                double area = Area(prev, current, next);
    
                for (int x = 0; x < indices.Count; x++)
                {
                    //checks to make sure there aren't duplicates
                    if (indices[x] == currentIndex || indices[x] == nextIndex || indices[x] == prevIndex)
                    {
                        continue;
                    }
    
                    //The current point to check with
                    Vector2 checkPoint = vertices[indices[x]];
    
                    //Find the areas of the three triangles between current, next, prev, and the check point
                    double a1 = Area(prev, current, checkPoint);
                    double a2 = Area(current, next, checkPoint);
                    double a3 = Area(prev, next, checkPoint);
    
                    //if each area adds up to the total, then the point must be inside the triangle
                    if (a1 + a2 + a3 == area)
                    {
                        valid = false;
                        break;
                    }
                }
    
                if (valid)
                {
                    //add the triangle to the list of triangles
                    triangles.Add(prevIndex);
                    triangles.Add(currentIndex);
                    triangles.Add(nextIndex);
    
                    //remove the current index from consideration
                    indices.Remove(currentIndex);
                    //decrement i to prevent index out of bounds errors
                    i--;
                }
            }

            count++;
        }
        
        //add the final triangle
        triangles.Add(indices[0]);
        triangles.Add(indices[1]);
        triangles.Add(indices[2]);
    }

    //Returns the area of the triangle between the given points
    private double Area(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return Math.Abs((p1.x*(p2.y - p3.y) + p2.x*(p3.y - p1.y) + p3.x*(p1.y - p2.y)) / 2.0);
    }

    //Once the front side is done, it adds the reverse side to the triangles array
    private void AddOtherSide()
    {
        int indexOffset = vertices.Count;
        int vertexLimit = vertices.Count;
        int triangleLimit = triangles.Count;

        for (int i = 0; i < vertexLimit; i++)
        {
            //Add the back side vertex to vertices
            vertices.Add(vertices[i] + new Vector3(0, 0, width));
        }

        //Add the back side triangles in reverse
        for (int i = triangleLimit - 1; i >= 0; i--)
        {
            triangles.Add(triangles[i] + indexOffset);
        }
    }

    //Adds the sides to connect the front and back faces
    private void AddSides()
    {
        for (int i = 0; i < frontVertices.Count; i++)
        {
            int current = i;
            int next = current + 1;
            int backCurrent = i + frontVertices.Count - 1;
            int backNext = backCurrent + 1;

            triangles.Add(current);
            triangles.Add(backNext);
            triangles.Add(next);

            triangles.Add(backNext);
            triangles.Add(current);
            triangles.Add(backCurrent);
        }
    }

    //creates a new vertex for each triangle index, allowing the final object to
    //appear more cohesive. This is based on how Unity applies lighting over complex shapes
    private void DuplicateVertices()
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            finalVertices.Add(vertices[triangles[i]]);
            finalTriangles.Add(i);
        }
    }

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

    //Checks if any of the indices are too large
    private void CheckForMeshErrors()
    {
        for (int i = 0; i < finalTriangles.Count; i++)
        {
            if (finalTriangles[i] > finalVertices.Count - 1)
            {
                Debug.Log(i + " I'm too big for this: " + finalTriangles[i]);
            }
        }
    }

    private void TransformVerticies(){
        for (int i = 0; i < finalVertices.Count; i++){
//            finalVertices[i] = calc.CalculateNewPosition(finalVertices[i]);
        }
    }

    //Create the mesh
    private void UpdateMesh()
    {
        MeshCollider meshCollider = thingToCut.GetComponent<MeshCollider>();
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

    public Follower GetFollower()
    {
        return follower;
    }

    public GameObject GetThingToCutCopy()
    {
        return thingToCutCopy;
    }
    public GameObject GetThingToCut(){
        return thingToCut;
    }

    public float GetWidth()
    {
        return width;
    }

    public void SetThingToCut(GameObject newThing)
    {
        if (newThing.GetComponent<ThingToCut>() == null) {
            throw new ArgumentException();
        }
        thingToCut = newThing;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Segment
{
    Vector2 point1;
    Vector2 point2;
    public Segment(Vector2 one, Vector2 two)
    {
        point1 = one;
        point2 = two;
    }

    public Segment(Vector3 one, Vector3 two)
    {
        point1 = new Vector2 (one.x, one.y);
        point2 = new Vector2 (two.x, two.y);
    }

    public bool IsIntersection(Segment other)
    {
        Vector2 oPoint1 = other.GetPoint1();
        Vector2 oPoint2 = other.GetPoint2();

        int o1 = Orientation(point1, point2, oPoint1);
        int o2 = Orientation(point1, point2, oPoint2);
        int o3 = Orientation(oPoint1, oPoint2, point1);
        int o4 = Orientation(oPoint1, oPoint2, point2);

        //If any points are shared
        if (point1 == oPoint1 || point1 == oPoint2 || point2 == oPoint1 || point2 == oPoint2)
        {
            return false;
        }
        
        //General Case
        if (o1 != o2 && o3 != o4)
        {
            //Debug.Log("(" + point1 +", " + point2 + ") int (" + oPoint1 + ", " + oPoint2 + ") GC");
            return true;
        }

        //Special Cases
        //point1, point2, and oPoint1 are collinear and oPoint1 lies on segment point1-point2
        if (o1 == 0 && OnSegment(point1, oPoint1, point2)) { 
            //Debug.Log("(" + point1 +", " + point2 + ") int (" + oPoint1 + ", " + oPoint2 + ") SC1");
            return true; }

        //point1, point2, and oPoint2 are collinear and oPoint2 lies on segment point1-point2
        if (o2 == 0 && OnSegment(point1, oPoint2, point2)) { 
            //Debug.Log("(" + point1 +", " + point2 + ") int (" + oPoint1 + ", " + oPoint2 + ") SC2");
            return true; }

        //oPoint1, oPoint2, and point2 are collinear and point2 lies on segment oPoint1-oPoint2
        if (o3 == 0 && OnSegment(oPoint1, point1, oPoint2)) { 
            //Debug.Log("(" + point1 +", " + point2 + ") int (" + oPoint1 + ", " + oPoint2 + ") SC3");
            return true; }

        //oPoint1, oPoint2, and point2 are collinear and point2 lies on segment oPoint1-oPoint2
        if (o4 == 0 && OnSegment(oPoint1, point2, oPoint2)) { 
            //Debug.Log("(" + point1 +", " + point2 + ") int (" + oPoint1 + ", " + oPoint2 + ") SC4");
            return true; }

        return false; //Doesn't fall into any of the above cases
    }

    //Checks if p2 lies on segment p1-p3
    static bool OnSegment(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        if (p2.x <= Math.Max(p1.x, p3.x) && p2.x >= Math.Min(p1.x, p3.x) &&
            p2.y <= Math.Max(p1.y, p3.y) && p2.y >= Math.Min(p1.y, p3.y))
        {
            return true;
        }
        return false;
    }

    public static int Orientation(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float val = (p2.y - p1.y) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.y - p2.y);

        if (val == 0) { return 0; }

        //if clockwise
        if (val > 0)
        {
            return 1;
        }
        //if counterclockwise
        return 2;
    }

    public Vector2 GetPoint1()
    {
        return point1;
    }

    public Vector2 GetPoint2()
    {
        return point2;
    }
}


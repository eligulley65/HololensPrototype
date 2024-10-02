/*
Created by Elijah Gulley

Holder objects created by PathNodeCreator

They are created in order to more easily track what positions should be followed
The follower script takes in a list of these and moves between them
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    //The worldposition tracks a given positon
    //As it is currently used this postion is a mouse position,
    //but this will change
    private Vector3 worldPosition;

    //Initialize each value on creation
    public PathNode(Vector3 worldPosition)
    {
        this.worldPosition = worldPosition;
    }

    //calls the other constructor with a rank of -1
    //use only for temp PathNodes
    //currently unused
    //public PathNode(Vector3 worldPosition) : this(worldPosition, -1) {} 

    //Get methods. No set methods should ever be used for these objects
    public Vector3 GetWorldPosition()
    {
        return worldPosition;
    }
}
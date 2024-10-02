/*
Created By Elijah Gulley

This object takes in the pathNodeList from PathNodeCreator and moves between each position
stored in the PathNodes. Each move is set by a frame delay, and it moves around the list 
a number of times set by the passes variable. It's only able to move once an event is invoked
by PathNodeCreator.

*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    //Time delay in frames between moves.
    const int TIME_BETWEEN_MOVES = 60; //measured in frames

    public event EventHandler OnPathCompleted;

    CutManager cutManager;

    //pathNodeList taken from PathNodeCreator
    List<PathNode> pathNodeList = new List<PathNode>();
    //variable to track the rank of the current PathNode to make sure
    //the movement follows the correct order
    int currentPathNodeRank = 0;
    //Ticks down each frame the object isn't moving, and is reset once it moves
    int timer = TIME_BETWEEN_MOVES;
    //Whether or not the object can move. Exists to prevent Null Reference exceptions
    bool ableToMove = false;
    bool shouldInvokeEvent = false;
    //Number of times the follower loops
    [SerializeField] int numberOfPasses;

    void Awake()
    {
        cutManager = FindObjectOfType<CutManager>();
    }

    //Listen to the PathNodeCreationFinished event to prevent Null Reference exceptions
    void Start()
    {
        PathNodeCreator.Instance.OnPathNodeCreationFinished += PathNodeCreator_OnPathNodeCreationFinished;
        cutManager.OnInvalidShape += CutManager_OnInvalidShape;
    }

    //If not able to move, return
    //If able to move, decrement timer until it hits 0. Then, move and reset the timer
    void Update()
    {
        if (!ableToMove) { return; }

        if (timer <= 0)
        {
            timer = TIME_BETWEEN_MOVES;
            MoveToNext();
        }
        else
        {
            timer--;
        }
    }

    //When the event is invoked set pathNodeList and ableToMove
    private void PathNodeCreator_OnPathNodeCreationFinished(object sender, EventArgs e)
    {
        pathNodeList = PathNodeCreator.Instance.GetPathNodeList();
        ableToMove = true;
    }

    private void CutManager_OnInvalidShape(object sender, EventArgs e)
    {
        shouldInvokeEvent = false;
    }

    //Method that finds the next PathNode in pathNodeList and moves to it.
    //Only cycles while passes > 0
    private void MoveToNext()
    {
        if (currentPathNodeRank >= pathNodeList.Count)
        {
            numberOfPasses--;
            if (numberOfPasses <= 0)
            {
                ableToMove = false;
                if (shouldInvokeEvent) { OnPathCompleted?.Invoke(this, EventArgs.Empty); }
                LineCreator.Instance.DeleteLine();
                Destroy(this.gameObject);
            }
            currentPathNodeRank = 0;
        }

        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if (i == currentPathNodeRank)
            {
                transform.position = pathNodeList[i].GetWorldPosition();
                currentPathNodeRank++;
                return;
            }
        }
    }
}
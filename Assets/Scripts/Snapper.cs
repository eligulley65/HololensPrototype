using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snapper : MonoBehaviour
{
    private bool canSnap = false;

    private void Start() {
        ThingToCut thingToCut = FindObjectOfType<ThingToCut>();
        thingToCut.OnManipEnded += ThingToCut_OnManipEnded;
        thingToCut.OnManipStarted += ThingToCut_OnManipStarted;
    }

    private void ThingToCut_OnManipStarted(object sender, EventArgs e)
    {
        canSnap = false;
    }


    private void ThingToCut_OnManipEnded(object sender, EventArgs e)
    {
        canSnap = true;
    }

    void OnTriggerStay(Collider other) 
    {
        if (other.gameObject.CompareTag("ThingToCut") && canSnap)
        {
            other.transform.position = transform.position;
            canSnap = false;
        }
    }
}

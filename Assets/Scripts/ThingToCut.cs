using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThingToCut : MonoBehaviour
{
    public EventHandler OnManipEnded;
    public EventHandler OnManipStarted;

    public void ManipulationEnded()
    {
        OnManipEnded?.Invoke(this, EventArgs.Empty);
    }

    public void ManipulationStarted()
    {
        OnManipStarted?.Invoke(this, EventArgs.Empty);
    }
}

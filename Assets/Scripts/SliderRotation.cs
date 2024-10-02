using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.HandCoach;
using UnityEngine;

public class SliderRotation : MonoBehaviour
{
    [SerializeField] PinchSlider slider;
    float maxRotation = -90f;
    float currentRot = 0f;

    public void SliderUpdated()
    {
        float fraction = slider.SliderValue;
        float destination = fraction * maxRotation;
        float gap = destination - currentRot;
        currentRot = destination;
        transform.Rotate(new Vector3(gap, 0f, 0f));
    }
} 
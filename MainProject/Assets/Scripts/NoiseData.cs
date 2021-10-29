using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NoiseData
{
    public bool enabled = true;
    public bool positiveOnly = false;
    public float scale = 50f;
    public int octaves = 4;
    [Range(0, 1)]
    public float persistance = 0.5f;
    public float lacunarity = 2f;
    public float overallMult = 1f;

    public float xOffset = 0;
    public float zOffset = 0;

    public AnimationCurve animationCurve = new AnimationCurve();
}

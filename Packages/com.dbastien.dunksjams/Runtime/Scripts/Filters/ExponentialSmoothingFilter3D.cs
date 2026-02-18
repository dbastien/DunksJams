using System;
using UnityEngine;

public class ExponentialSmoothingFilter3D : IFilter3D
{
    public float Alpha = 0.5f;

    public Vector3 CurrentValue => previousSample;
    private bool previousSampleSet;
    private Vector3 previousSample;

    public object Clone() => MemberwiseClone();

    public ExponentialSmoothingFilter3D(float defaultAlpha)
    {
        if (defaultAlpha is < 0f or > 1.0f)
            throw new ArgumentOutOfRangeException("defaultAlpha");

        Alpha = defaultAlpha;
    }

    public void Update(Vector3 s)
    {
        if (previousSampleSet) { previousSample = Alpha * s + (1.0f - Alpha) * previousSample; }
        else
        {
            previousSampleSet = true;
            previousSample = s;
        }
    }

    public void Reset()
    {
        previousSample = new Vector3();
        previousSampleSet = false;
    }
}
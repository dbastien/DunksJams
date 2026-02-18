using System;

public class ExponentialSmoothingFilter : IFilter1D
{
    public float Alpha;
    public float CurrentValue => previousSample;

    private bool previousSampleSet;
    private float previousSample;

    public ExponentialSmoothingFilter(float defaultAlpha)
    {
        if (defaultAlpha is < 0f or > 1.0f)
            throw new ArgumentOutOfRangeException("defaultAlpha");

        Alpha = defaultAlpha;
    }

    public void Update(float s)
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
        previousSample = 0.0f;
        previousSampleSet = false;
    }
}
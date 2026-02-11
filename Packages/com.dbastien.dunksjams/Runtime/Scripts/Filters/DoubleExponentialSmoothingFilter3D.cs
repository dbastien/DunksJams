using UnityEngine;

class DoubleExponentialSmoothingFilter3D : IFilter3D
{
    private float smoothing = 0.33f;
    
    private float correction = 0.25f;
    //private float prediction = 0.0f;
    private float jitterRadius = 0.07f;
    //private float maxDeviationRadius = 0.05f;
    
    //history data
    private Vector3 rawPosition;
    private Vector3 filteredPosition;
    private Vector3 trend;
    private int frameCount;

    public object Clone() => MemberwiseClone();

    public Vector3 CurrentValue => filteredPosition;

    public void Update(Vector3 s)
    {
        Vector3 vPrevFilteredPosition = filteredPosition;
        Vector3 vRawPosition = s;
        Vector3 vFilteredPosition;
        Vector3 vDiff;
        
        // Initial start values
        if (frameCount == 0)
        {
           vFilteredPosition = vRawPosition;
           ++frameCount;
        }
        else if (frameCount == 1)
        {
            vFilteredPosition = (vRawPosition + rawPosition) * 0.5f;
            vDiff = vFilteredPosition - vPrevFilteredPosition;
            ++frameCount;
        }
        else
        {
            // Apply jitter filter
            float diff = Mathf.Abs((rawPosition - vPrevFilteredPosition).magnitude);

            if (diff <= jitterRadius)
            {
                vFilteredPosition = vRawPosition * (diff / jitterRadius) + vPrevFilteredPosition * (1.0f - diff / jitterRadius);
            }
            else
            {
                vFilteredPosition = vRawPosition;
            }

            // Apply dampening filter
            vFilteredPosition = vFilteredPosition * (1.0f - smoothing) + (vPrevFilteredPosition * smoothing);
        }

        rawPosition = vRawPosition;
        trend = Vector3.zero;
        filteredPosition = vFilteredPosition;
    }

    public void Reset()
    {
    }
}

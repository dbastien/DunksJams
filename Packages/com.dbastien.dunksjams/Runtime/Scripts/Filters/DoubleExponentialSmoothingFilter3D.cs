using UnityEngine;

public class DoubleExponentialSmoothingFilter3D : IFilter3D
{
    float smoothing = 0.33f;
    float correction = 0.25f;
    float jitterRadius = 0.07f;

    Vector3 filteredPosition;
    Vector3 trend;
    int frameCount;

    public object Clone() => MemberwiseClone();

    public Vector3 CurrentValue => filteredPosition;

    public void Update(Vector3 rawPosition)
    {
        if (frameCount == 0)
        {
            filteredPosition = rawPosition;
            trend = Vector3.zero;
            frameCount = 1;
            return;
        }

        Vector3 prevFilteredPosition = filteredPosition;
        Vector3 prevTrend = trend;

        // Jitter filter
        Vector3 diff = rawPosition - prevFilteredPosition;
        if (diff.magnitude <= jitterRadius)
        {
            rawPosition = Vector3.Lerp(prevFilteredPosition, rawPosition, diff.magnitude / jitterRadius);
        }

        // Double Exponential Smoothing (Holt-Winters)
        // Level
        filteredPosition = Vector3.Lerp(prevFilteredPosition + prevTrend, rawPosition, smoothing);
        // Trend
        trend = Vector3.Lerp(prevTrend, filteredPosition - prevFilteredPosition, correction);

        ++frameCount;
    }

    public void Reset()
    {
        filteredPosition = Vector3.zero;
        trend = Vector3.zero;
        frameCount = 0;
    }
}
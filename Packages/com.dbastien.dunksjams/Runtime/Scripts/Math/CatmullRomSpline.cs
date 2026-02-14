using UnityEngine;

/// <summary>
/// Catmull-Rom spline utilities: point interpolation, arc length measurement,
/// point-at-distance lookup, and spline bounds computation.
/// Extracted from LandscapeBuilder's LBPath.
/// </summary>
public static class CatmullRomSpline
{
    /// <summary>
    /// Evaluate a Catmull-Rom spline at parameter t (0-1) between p1 and p2.
    /// p0 and p3 are control points before/after the segment.
    /// </summary>
    public static Vector3 Evaluate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        return 0.5f * ((2f * p1)
            + (-p0 + p2) * t
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
            + (-p0 + 3f * p1 - 3f * p2 + p3) * (t2 * t));
    }

    /// <summary>
    /// Approximate the arc length of a spline segment between minT and maxT
    /// using the given number of sample intervals.
    /// </summary>
    public static float MeasureSegmentLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        float minT = 0f, float maxT = 1f, int intervals = 100)
    {
        float length = 0f;
        float range = maxT - minT;
        var prev = Evaluate(p0, p1, p2, p3, minT);

        for (int i = 1; i <= intervals; i++)
        {
            float t = (float)i / intervals * range + minT;
            var current = Evaluate(p0, p1, p2, p3, t);
            length += Vector3.Distance(prev, current);
            prev = current;
        }

        return length;
    }

    /// <summary>
    /// Find the point at a given distance along a spline segment
    /// by iteratively refining the t parameter.
    /// </summary>
    public static Vector3 FindPointAtDistance(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        float distance, float segmentLength, float maxError = 0.001f)
    {
        float tValue = distance / segmentLength;

        for (int iteration = 0; iteration < 1000; iteration++)
        {
            float measured = MeasureSegmentLength(p0, p1, p2, p3, 0f, tValue, 100);
            float nextT = tValue + (distance - measured) / segmentLength;

            if (float.IsNaN(nextT))
            {
                tValue = distance / segmentLength;
                break;
            }

            float variance = Mathf.Abs(tValue - nextT);
            tValue = nextT;

            if (variance < maxError) break;
        }

        return Evaluate(p0, p1, p2, p3, tValue);
    }

    /// <summary>
    /// Get the 2D (xz-plane) bounds of two spline arrays (e.g., left and right edges).
    /// Returns Vector4(minX, minZ, maxX, maxZ). Assumes equal-length arrays.
    /// </summary>
    public static Vector4 GetSplineBounds(Vector3[] splineLeft, Vector3[] splineRight)
    {
        var bounds = new Vector4(float.PositiveInfinity, float.PositiveInfinity,
            float.NegativeInfinity, float.NegativeInfinity);

        int count = splineLeft?.Length ?? 0;
        if (count != (splineRight?.Length ?? 0)) return bounds;

        for (int i = 0; i < count; i++)
        {
            UpdateBounds(ref bounds, splineLeft[i]);
            UpdateBounds(ref bounds, splineRight[i]);
        }

        return bounds;
    }

    static void UpdateBounds(ref Vector4 bounds, Vector3 pt)
    {
        if (pt.x < bounds.x) bounds.x = pt.x;
        if (pt.z < bounds.y) bounds.y = pt.z;
        if (pt.x > bounds.z) bounds.z = pt.x;
        if (pt.z > bounds.w) bounds.w = pt.z;
    }
}

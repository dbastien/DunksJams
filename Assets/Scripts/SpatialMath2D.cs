using UnityEngine;

public static class SpatialMath2D
{
    public static Vector2 ClosestPointOnLineSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        return a + ab * Mathf.Clamp01(t);
    }
}
using UnityEngine;

public static class SpatialMath2D
{
    public static float AngleBetweenPoints(Vector2 a, Vector2 b) =>
        Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;

    public static Vector2 ReflectPointAcrossLine(Vector2 p, Vector2 a, Vector2 b) =>
        LineSegment2D.ClosestPointOnLineSegment(p, a, b) * 2 - p;
}
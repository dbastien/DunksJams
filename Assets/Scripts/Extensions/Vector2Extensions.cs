using System;
using UnityEngine;

//todo: add "ref this" as well as "this" methods

public static class Vector2Extensions
{
    public static Vector2 LerpUnclamped(this Vector2 l, Vector2 r, float t) => l + t * (r - l);

    public static Vector2 Abs(this Vector2 v) => new(MathF.Abs(v.x), MathF.Abs(v.y));
    public static Vector2 Round(this Vector2 v) => new(MathF.Round(v.x), MathF.Round(v.y));
    public static Vector2 Floor(this Vector2 v) => new(MathF.Floor(v.x), MathF.Floor(v.y));
    public static Vector2 Ceil(this Vector2 v) => new(Mathf.Ceil(v.x), Mathf.Ceil(v.y));
    
    public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max) =>
        new(Mathf.Clamp(v.x, min.x, max.x),
            Mathf.Clamp(v.y, min.y, max.y));

    public static float PerpDot(this Vector2 l, Vector2 r) => l.x * r.y - l.y * r.x;
    
    public static Vector2 RotateTowards(this Vector2 v, Vector2 target, float maxRadiansDelta, float maxMagnitudeDelta)
    {
        var angleDelta = Mathf.Clamp(Vector2.SignedAngle(v, target), -Mathf.Rad2Deg * maxRadiansDelta, Mathf.Rad2Deg * maxRadiansDelta);
        var newAngle = Mathf.Atan2(v.y, v.x) + Mathf.Deg2Rad * angleDelta;

        var newDir = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
        var newMagnitude = Mathf.MoveTowards(v.magnitude, target.magnitude, maxMagnitudeDelta);

        return newDir * newMagnitude;
    }
    
    public static Vector2 SafeNormalize(this Vector2 v) => v == Vector2.zero ? Vector2.zero : v.normalized;

    public static float ManhattanDistance(this Vector2 a, Vector2 b) =>
        MathF.Abs(a.x - b.x) + MathF.Abs(a.y - b.y);

    public static float ClampedDistance(this Vector2 a, Vector2 b, float maxDist)
    {
        float distSquared = (a - b).sqrMagnitude;
        return distSquared > maxDist * maxDist ? maxDist : MathF.Sqrt(distSquared);
    }
    
    public static Vector2 Rotate(this Vector2 v, float radians)
    {
        float sin = MathF.Sin(radians);
        float cos = MathF.Cos(radians);

        float x = v.x;
        float y = v.y;

        v.x = x * cos - y * sin;
        v.y = x * sin + y * cos;
        
        return v;
    }

    public static Vector2 SinCos(float radians) => new(MathF.Sin(radians), MathF.Cos(radians));
    
    public static Vector2 ClosestPointOnLineSegment(this Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        return a + ab * Mathf.Clamp01(t);
    }
}
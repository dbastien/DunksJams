using System;
using UnityEngine;

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
        float angleDelta = Mathf.Clamp(Vector2.SignedAngle(v, target), -Mathf.Rad2Deg * maxRadiansDelta,
            Mathf.Rad2Deg * maxRadiansDelta);
        float newAngle = Mathf.Atan2(v.y, v.x) + Mathf.Deg2Rad * angleDelta;

        var newDir = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
        float newMagnitude = Mathf.MoveTowards(v.magnitude, target.magnitude, maxMagnitudeDelta);

        return newDir * newMagnitude;
    }

    public static Vector2 SafeNormalize(this Vector2 v) =>
        v == Vector2.zero ? Vector2.zero : v.normalized;

    public static float ManhattanDistance(this Vector2 a, Vector2 b) =>
        MathF.Abs(a.x - b.x) + MathF.Abs(a.y - b.y);

    public static float ClampedDistance(this Vector2 a, Vector2 b, float maxDist)
    {
        float distSquared = (a - b).sqrMagnitude;
        return distSquared > maxDist * maxDist ? maxDist : MathF.Sqrt(distSquared);
    }

    public static Vector2 RotateSinCon(this Vector2 v, float radians)
    {
        float sin = MathF.Sin(radians);
        float cos = MathF.Cos(radians);

        float x = v.x;
        float y = v.y;

        v.x = x * cos - y * sin;
        v.y = x * sin + y * cos;

        return v;
    }

    public static Vector2 SinCos(float radians) =>
        new(MathF.Sin(radians), MathF.Cos(radians));

    public static bool Approximately(this Vector2 a, Vector2 b) =>
        Vector2.SqrMagnitude(a - b) < 0.0001f ? true : false;

    public static Vector3 MergeValues(this Vector2 v, int insertIndex, float insertValue) =>
        insertIndex switch
        {
            0 => new Vector3(insertValue, v.x, v.y),
            1 => new Vector3(v.x, insertValue, v.y),
            2 => new Vector3(v.x, v.y, insertValue),
            _ => throw new Exception("index out of range")
        };

    public static float DistanceTo(this Vector2 f1, Vector2 f2) => (f1 - f2).magnitude;
    public static Vector2 Clamp01(this Vector2 f) => new(Mathf.Clamp01(f.x), Mathf.Clamp01(f.y));
    public static float ProjectOn(this Vector2 v, Vector2 on) => Vector3.Project(v, on).magnitude;
    public static float AngleTo(this Vector2 v, Vector2 to) => Vector2.Angle(v, to);
    public static Vector2 Rotate(this Vector2 v, float deg) => Quaternion.AngleAxis(deg, Vector3.forward) * v;

    public static float InverseLerp(this Vector2 v, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 av = v - a;
        return Vector2.Dot(av, ab) / Vector2.Dot(ab, ab);
    }
}
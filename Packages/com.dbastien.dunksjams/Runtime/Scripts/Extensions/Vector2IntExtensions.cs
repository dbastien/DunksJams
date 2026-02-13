using System;
using UnityEngine;

public static class Vector2IntExtensions
{
    public static Vector2Int up => new(0, 1);
    public static Vector2Int down => new(0, -1);
    public static Vector2Int left => new(-1, 0);
    public static Vector2Int right => new(1, 0);

    public static int Dot(this Vector2Int l, Vector2Int r) => l.x * r.x + l.y * r.y;
    public static int DotOne(this Vector2Int v) => v.x * v.y;
    public static int PerpDot(this Vector2Int l, Vector2Int r) => l.x * r.y - l.y * r.x;

    public static Vector2Int Abs(this Vector2Int v) => new(Mathf.Abs(v.x), Mathf.Abs(v.y));
    public static Vector2Int Sign(this Vector2Int v) => new(MathF.Sign(v.x), MathF.Sign(v.y));

    public static Vector2Int Min(this Vector2Int l, Vector2Int r) =>
        new(Mathf.Min(l.x, r.x), Mathf.Min(l.y, r.y));

    public static Vector2Int Max(this Vector2Int l, Vector2Int r) =>
        new(Mathf.Max(l.x, r.x), Mathf.Max(l.y, r.y));

    public static Vector2Int Clamp(this Vector2Int v, Vector2Int min, Vector2Int max) =>
        new(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));

    public static Vector2Int Clamp(this Vector2Int v, int min, int max) =>
        new(Mathf.Clamp(v.x, min, max), Mathf.Clamp(v.y, min, max));

    public static Vector2Int ClosestPowerOfTwo(this Vector2Int v) =>
        new(Mathf.ClosestPowerOfTwo(v.x), Mathf.ClosestPowerOfTwo(v.y));

    public static Vector2Int NextPowerOfTwo(this Vector2Int v) =>
        new(Mathf.NextPowerOfTwo(v.x), Mathf.NextPowerOfTwo(v.y));

    public static Vector2 Normalized(this Vector2Int v)
    {
        var mag = Mathf.Sqrt(v.x * v.x + v.y * v.y);
        return mag > 0 ? new Vector2(v.x / mag, v.y / mag) : Vector2.zero;
    }

    public static float Distance(this Vector2Int a, Vector2Int b) =>
        MathF.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));

    public static int DistanceSquared(this Vector2Int a, Vector2Int b) =>
        (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);

    public static float ManhattanDistance(this Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    public static float OctileDistance(this Vector2Int a, Vector2Int b, float straightCost = 1f,
        float diagCost = 1.41421356237f)
    {
        var dx = Mathf.Abs(a.x - b.x);
        var dy = Mathf.Abs(a.y - b.y);
        return straightCost * (dx + dy) + (diagCost - 2 * straightCost) * Mathf.Min(dx, dy);
    }

    public static int ChebyshevDistance(this Vector2Int a, Vector2Int b) =>
        Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

    public static float Angle(this Vector2Int a, Vector2Int b) =>
        Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;

    public static Vector2Int Reflect(this Vector2Int vector, Vector2Int normal) =>
        vector - 2 * vector.Dot(normal) / normal.Dot(normal) * normal;

    public static Vector2Int NearestCardinalOrDiagonalDirection(this Vector2Int from, Vector2Int to)
    {
        var dx = to.x - from.x;
        var dy = to.y - from.y;
        var absDx = Mathf.Abs(dx);
        var absDy = Mathf.Abs(dy);

        return absDx > absDy ? new Vector2Int((int)Mathf.Sign(dx), 0) :
            absDy > absDx ? new Vector2Int(0, (int)Mathf.Sign(dy)) :
            new Vector2Int((int)Mathf.Sign(dx), (int)Mathf.Sign(dy));
    }
}
using System;
using UnityEngine;

public static class Vector2IntExtensions
{
    public static int Dot(this Vector2Int l, Vector2Int r) => l.x * r.x + l.y * r.y;
    public static int DotOne(this Vector2Int v) => v.x * v.y;
    public static int PerpDot(this Vector2Int l, Vector2Int r) => l.x * r.y - l.y * r.x;

    public static Vector2Int Min(this Vector2Int l, Vector2Int r) => new(Mathf.Min(l.x, r.x), Mathf.Min(l.y, r.y));
    public static Vector2Int Max(this Vector2Int l, Vector2Int r) => new(Mathf.Max(l.x, r.x), Mathf.Max(l.y, r.y));

    public static Vector2Int Clamp(this Vector2Int v, Vector2Int min, Vector2Int max) => 
        new(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));

    public static Vector2Int ClosestPowerOfTwo(this Vector2Int v) => new(Mathf.ClosestPowerOfTwo(v.x), Mathf.ClosestPowerOfTwo(v.y));
    
    public static float Distance(this Vector2Int a, Vector2Int b) =>
        MathF.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));

    public static int ManhattanDistance(this Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
}
using UnityEngine;

public static class Vector3IntExtensions
{
    public static readonly Vector3Int Forward = new(0, 0, 1);
    public static readonly Vector3Int Back = new(0, 0, -1);

    public static Vector3Int Abs(this Vector3Int v) =>
        new(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    public static int Dot(Vector3Int l, Vector3Int r) => l.x * r.x + l.y * r.y + l.z * r.z;
    public static int DotOne(this Vector3Int v) => v.x * v.y * v.z;

    public static Vector3Int Cross(Vector3Int l, Vector3Int r) =>
        new(l.y * r.z - l.z * r.y,
            l.z * r.x - l.x * r.z,
            l.x * r.y - l.y * r.x);

    public static Vector3Int Min(Vector3Int l, Vector3Int r) =>
        new(Mathf.Min(l.x, r.x), Mathf.Min(l.y, r.y), Mathf.Min(l.z, r.z));

    public static Vector3Int Max(Vector3Int l, Vector3Int r) =>
        new(Mathf.Max(l.x, r.x), Mathf.Max(l.y, r.y), Mathf.Max(l.z, r.z));

    public static Vector3Int Clamp(Vector3Int v, Vector3Int min, Vector3Int max) =>
        new(Mathf.Clamp(v.x, min.x, max.x),
            Mathf.Clamp(v.y, min.y, max.y),
            Mathf.Clamp(v.z, min.z, max.z));

    public static Vector3Int ClosestPowerOfTwo(Vector3Int v) =>
        new(Mathf.ClosestPowerOfTwo(v.x),
            Mathf.ClosestPowerOfTwo(v.y),
            Mathf.ClosestPowerOfTwo(v.z));

    public static int CubicToLinearIndex(Vector3Int v, Vector3Int size) =>
        v.x +
        v.y * size.x +
        v.z * size.x * size.y;

    public static Vector3Int LinearToCubicIndex(int v, Vector3Int size) =>
        new(v % size.x,
            v / size.x % size.y,
            v / (size.x * size.y) % size.z);

    public static int ManhattanDistance(Vector3Int l, Vector3Int r) =>
        Mathf.Abs(l.x - r.x) + Mathf.Abs(l.y - r.y) + Mathf.Abs(l.z - r.z);
}
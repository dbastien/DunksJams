using System;
using UnityEngine;

//todo: add "ref this" as well as "this" methods

public static class Vector3Extensions
{
    public static Vector3 LerpUnclamped(this Vector3 l, Vector3 r, float t) => l + t * (r - l);

    public static Vector3 Abs(this Vector3 v) => new(MathF.Abs(v.x), MathF.Abs(v.y), MathF.Abs(v.z));
    public static Vector3 Round(this Vector3 v) => new(MathF.Round(v.x), MathF.Round(v.y), MathF.Round(v.z));
    public static Vector3 Floor(this Vector3 v) => new(MathF.Floor(v.x), MathF.Floor(v.y), MathF.Floor(v.z));
    public static Vector3 Ceil(this Vector3 v) => new(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z));

    public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max) =>
        new(Mathf.Clamp(v.x, min.x, max.x),
            Mathf.Clamp(v.y, min.y, max.y),
            Mathf.Clamp(v.z, min.z, max.z));

    public static Vector3 Scaled(this Vector3 v, Vector3 scale) => 
        new(v.x * scale.x, v.y * scale.y, v.z * scale.z);

    public static Vector3 GetFacingVector(this Vector3 v, Vector3 toPos) => (toPos - v).normalized;

    public static Vector3 GetFacingVectorExclusive(this Vector3 v, Vector3 toPos, int indexToExclude)
    {
        Vector3 facing = v.GetFacingVector(toPos);
        return facing.SetIndex(indexToExclude, 0).normalized;
    }

    public static Vector3 GetFacingVectorAroundAxis(this Vector3 v, Vector3 toPos, Vector3 axis)
    {
        Vector3 facing = v.GetFacingVector(toPos);
        Vector3 right = Vector3.Cross(axis, facing);
        Vector3 forward = Vector3.Cross(right, axis);
        return forward.normalized;
    }

    public static Vector3 GetFacingVectorAroundAxis2(this Vector3 v, Vector3 toPos, Vector3 axis)
    {
        Vector3 facing = v.GetFacingVector(toPos);
        Vector3 projectedFacing = Vector3.ProjectOnPlane(facing, axis);
        return projectedFacing != Vector3.zero ? projectedFacing.normalized : Vector3.zero;
    }

    public static Vector3 SetIndex(this Vector3 v, int index, float val) =>
        index switch
        {
            0 => new(val, v.y, v.z),
            1 => new(v.x, val, v.z),
            2 => new(v.x, v.y, val),
            _ => throw new("index out of range")
        };
    
    public static Vector3 ClosestPointOnLineSegment(this Vector3 p, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        var t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        return a + ab * Mathf.Clamp01(t);
    }
}

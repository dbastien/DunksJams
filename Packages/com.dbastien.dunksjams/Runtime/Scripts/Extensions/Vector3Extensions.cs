using System;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 Abs(this Vector3 v) => new(MathF.Abs(v.x), MathF.Abs(v.y), MathF.Abs(v.z));
    public static Vector3 Round(this Vector3 v) => new(MathF.Round(v.x), MathF.Round(v.y), MathF.Round(v.z));
    public static Vector3 Ceil(this Vector3 v) => new(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z));
    public static Vector3 Floor(this Vector3 v) => new(MathF.Floor(v.x), MathF.Floor(v.y), MathF.Floor(v.z));

    public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max) =>
        new(Mathf.Clamp(v.x, min.x, max.x),
            Mathf.Clamp(v.y, min.y, max.y),
            Mathf.Clamp(v.z, min.z, max.z));

    public static Vector3 LerpUnclamped(this Vector3 l, Vector3 r, float t) => Vector3.LerpUnclamped(l, r, t);

    public static Vector3 Scaled(this Vector3 v, Vector3 scale) =>
        new(v.x * scale.x, v.y * scale.y, v.z * scale.z);

    public static Vector3 GetFacingVector(this Vector3 v, Vector3 toPos) => (toPos - v).normalized;

    public static Vector3 GetFacingVectorExclusive(this Vector3 v, Vector3 toPos, int indexToExclude) =>
        v.GetFacingVector(toPos).SetIndex(indexToExclude, 0).normalized;

    public static Vector3 GetFacingVectorAroundAxis(this Vector3 v, Vector3 toPos, Vector3 axis)
    {
        var facing = v.GetFacingVector(toPos);
        var right = Vector3.Cross(axis, facing);
        var forward = Vector3.Cross(right, axis);
        return forward.normalized;
    }

    public static Vector3 GetFacingVectorAroundAxis2(this Vector3 v, Vector3 toPos, Vector3 axis)
    {
        var facing = v.GetFacingVector(toPos);
        var projectedFacing = Vector3.ProjectOnPlane(facing, axis);
        return projectedFacing != Vector3.zero ? projectedFacing.normalized : Vector3.zero;
    }

    public static Vector3 SetIndex(this Vector3 v, int index, float val) =>
        index switch
        {
            0 => new Vector3(val, v.y, v.z),
            1 => new Vector3(v.x, val, v.z),
            2 => new Vector3(v.x, v.y, val),
            _ => throw new Exception("index out of range")
        };

    public static float GetValueFromIndex(this Vector3 v, int index) =>
        index switch
        {
            0 => v.x,
            1 => v.y,
            2 => v.z,
            _ => throw new Exception("index out of range")
        };

    public static Vector2 GetValuesFromExclusionIndex(this Vector3 v, int excludeIndex) =>
        excludeIndex switch
        {
            0 => new Vector2(v.y, v.z),
            1 => new Vector2(v.x, v.z),
            2 => new Vector2(v.x, v.y),
            _ => throw new Exception("index out of range")
        };

    public static bool Approximately(this Vector3 a, Vector3 b) =>
        Vector3.SqrMagnitude(a - b) < 0.0001f ? true : false;
}
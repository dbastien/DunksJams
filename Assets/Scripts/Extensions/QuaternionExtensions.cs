using System;
using UnityEngine;

public static class QuaternionExtensions
{
    public static Quaternion LerpUnclamped(this Quaternion l, Quaternion r, float t)
    {
        Quaternion result = new(
            l.x + (r.x - l.x) * t,
            l.y + (r.y - l.y) * t,
            l.z + (r.z - l.z) * t,
            l.w + (r.w - l.w) * t
        );

        return result.normalized;
    }
    
    public static Quaternion SlerpUnclamped(this Quaternion l, Quaternion r, float t) => 
        Quaternion.SlerpUnclamped(l, r, t);

    public static Quaternion RotateTowards(this Quaternion from, Quaternion to, float maxDegreesDelta)
    {
        float angle = Quaternion.Angle(from, to);
        if (angle == 0f) return to;
    
        float t = MathF.Min(1f, maxDegreesDelta / angle);
        return Quaternion.Slerp(from, to, t);
    }
    
    public static Quaternion LookRotationSafe(this Quaternion q, Vector3 forward, Vector3 up)
    {
        if (forward == Vector3.zero) forward = Vector3.forward;
        if (up == Vector3.zero || forward == up) up = Vector3.up;

        return Quaternion.LookRotation(forward, up);
    }
}
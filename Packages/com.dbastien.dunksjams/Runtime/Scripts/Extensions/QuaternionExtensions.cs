using UnityEngine;

public static class QuaternionExtensions
{
    public static Quaternion LerpUnclamped(this Quaternion l, Quaternion r, float t)
    {
        var oneMinusT = 1f - t;
        Quaternion result = new(l.x * oneMinusT + r.x * t, l.y * oneMinusT + r.y * t, l.z * oneMinusT + r.z * t,
            l.w * oneMinusT + r.w * t);

        var mag = Mathf.Sqrt(result.x * result.x + result.y * result.y + result.z * result.z + result.w * result.w);
        if (mag > MathConsts.Epsilon_Float)
        {
            var invMag = 1f / mag;
            result.x *= invMag;
            result.y *= invMag;
            result.z *= invMag;
            result.w *= invMag;
        }

        return result;
    }

    public static Quaternion SlerpUnclamped(this Quaternion l, Quaternion r, float t) =>
        Quaternion.SlerpUnclamped(l, r, t);

    public static Quaternion RotateTowards(this Quaternion from, Quaternion to, float maxDegDelta)
    {
        var angle = Quaternion.Angle(from, to);
        if (angle < MathConsts.Epsilon_Float || maxDegDelta >= angle) return to;
        return Quaternion.Slerp(from, to, maxDegDelta / angle);
    }

    public static Quaternion LookRotationSafe(this Quaternion q, Vector3 forward, Vector3 up)
    {
        if (forward.sqrMagnitude < MathConsts.Epsilon_Float) forward = Vector3.forward;

        if (up.sqrMagnitude < MathConsts.Epsilon_Float ||
            Vector3.Dot(forward.normalized, up.normalized) > MathConsts.Threshold_VectorAlignment)
            up = Vector3.Cross(forward, Mathf.Abs(forward.x) > 0.9f ? Vector3.up : Vector3.right).normalized;

        return Quaternion.LookRotation(forward, up);
    }
}
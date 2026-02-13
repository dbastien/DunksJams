using System;
using UnityEngine;

public static class ProceduralRotation
{
    public static Quaternion RandomRotation()
    {
        var angle = Rand.Rad();
        var axis = RandomUnitVector();
        return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
    }

    public static Quaternion RandomRotationAroundAxis(Vector3 axis)
    {
        axis = axis.normalized;
        var angle = Rand.Rad();
        return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
    }

    public static Quaternion RandomRotationInCone(Vector3 forward, float maxAngleDegrees)
    {
        var angle = Rand.FloatRanged(0f, maxAngleDegrees);
        var axis = RandomUnitVector();
        var baseRotation = Quaternion.LookRotation(forward);
        return baseRotation * Quaternion.AngleAxis(angle, axis);
    }

    public static Quaternion RandomRotationInHemisphere(Vector3 up)
    {
        var direction = RandomDirectionInHemisphere(up);
        return Quaternion.LookRotation(direction, up);
    }

    public static Quaternion RandomRotationInRange(float minAngleDegrees, float maxAngleDegrees)
    {
        var angle = Rand.FloatRanged(minAngleDegrees, maxAngleDegrees);
        var axis = RandomUnitVector();
        return Quaternion.AngleAxis(angle, axis);
    }

    public static Quaternion RandomVariationOf(Quaternion baseRotation, float maxAngleVariation = 30f)
    {
        var variation = RandomRotationInRange(0f, maxAngleVariation);
        return baseRotation * variation;
    }

    public static Quaternion RandomVariationAroundAxis(Quaternion baseRotation, Vector3 axis,
        float maxAngleVariation = 30f)
    {
        var variation = RandomRotationAroundAxis(axis) *
                        Quaternion.AngleAxis(Rand.FloatRanged(-maxAngleVariation, maxAngleVariation), axis);
        return baseRotation * variation;
    }

    public static Quaternion WithNoise(Quaternion rotation, float noiseAmount = 10f)
    {
        var euler = rotation.eulerAngles;
        euler.x += Rand.FloatRanged(-noiseAmount, noiseAmount);
        euler.y += Rand.FloatRanged(-noiseAmount, noiseAmount);
        euler.z += Rand.FloatRanged(-noiseAmount, noiseAmount);
        return Quaternion.Euler(euler);
    }

    public static Quaternion Slerp(Quaternion a, Quaternion b, float t, Func<float, float> easing)
    {
        t = easing(t);
        return Quaternion.Slerp(a, b, t);
    }

    public static Quaternion LookAtWithRoll(Vector3 forward, Vector3 up) => Quaternion.LookRotation(forward, up);

    public static Quaternion LookAtRandomRoll(Vector3 forward)
    {
        var up = RandomUnitVector();
        var right = Vector3.Cross(forward, up);
        up = Vector3.Cross(right, forward);
        return Quaternion.LookRotation(forward, up);
    }

    public static Quaternion FromEulerRandom(float maxAngleDegrees = 360f) =>
        Quaternion.Euler(
            Rand.FloatRanged(-maxAngleDegrees, maxAngleDegrees),
            Rand.FloatRanged(-maxAngleDegrees, maxAngleDegrees),
            Rand.FloatRanged(-maxAngleDegrees, maxAngleDegrees)
        );

    public static Quaternion FromEulerRandomInRanges(Vector3 minAngles, Vector3 maxAngles) =>
        Quaternion.Euler(
            Rand.FloatRanged(minAngles.x, maxAngles.x),
            Rand.FloatRanged(minAngles.y, maxAngles.y),
            Rand.FloatRanged(minAngles.z, maxAngles.z)
        );

    public static Quaternion OscillatingRotation(float time, float frequency = 1f, Vector3 axis = default)
    {
        if (axis == default) axis = Vector3.up;
        var angle = Mathf.Sin(time * frequency * MathConsts.Tau) * 180f;
        return Quaternion.AngleAxis(angle, axis);
    }

    public static Quaternion SpiralRotation(float time, float frequency = 1f, float pitchSpeed = 1f)
    {
        var angle = time * frequency * 360f;
        var pitch = time * pitchSpeed * 360f;
        return Quaternion.Euler(pitch, angle, 0f);
    }

    public static Quaternion BlendRotations(params Quaternion[] rotations)
    {
        if (rotations.Length == 0) return Quaternion.identity;
        if (rotations.Length == 1) return rotations[0];

        var result = rotations[0];
        for (var i = 1; i < rotations.Length; i++) result = Quaternion.Slerp(result, rotations[i], 1f / (i + 1));
        return result;
    }

    public static Quaternion RandomFromArray(Quaternion[] rotations)
    {
        if (rotations.Length == 0) return Quaternion.identity;
        return rotations[Rand.IntRanged(0, rotations.Length)];
    }

    public static Vector3 RandomUnitVector()
    {
        var theta = Rand.Rad();
        var phi = Mathf.Acos(2f * Rand.Float() - 1f);
        var sinPhi = Mathf.Sin(phi);

        return new Vector3(
            sinPhi * Mathf.Cos(theta),
            sinPhi * Mathf.Sin(theta),
            Mathf.Cos(phi)
        );
    }

    public static Vector3 RandomDirectionInHemisphere(Vector3 up)
    {
        var randomDir = RandomUnitVector();
        if (Vector3.Dot(randomDir, up) < 0f) randomDir = -randomDir;
        return randomDir;
    }

    public static Vector3 RandomDirectionInCone(Vector3 forward, float maxAngleDegrees)
    {
        var angle = Rand.FloatRanged(0f, maxAngleDegrees * Mathf.Deg2Rad);
        var axis = RandomUnitVector();
        var perpendicular = Vector3.Cross(forward, axis);
        if (perpendicular == Vector3.zero) perpendicular = RandomUnitVector();
        perpendicular = perpendicular.normalized;

        return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, perpendicular) * forward;
    }

    public static float AngleBetween(Quaternion a, Quaternion b)
    {
        var delta = b * Quaternion.Inverse(a);
        delta.ToAngleAxis(out var angle, out _);
        return angle;
    }

    public static Quaternion ClampRotation(Quaternion rotation, Quaternion reference, float maxAngleDegrees)
    {
        var angle = AngleBetween(reference, rotation);
        if (angle > maxAngleDegrees) return Quaternion.RotateTowards(reference, rotation, maxAngleDegrees);
        return rotation;
    }

    public static Quaternion SmoothDamp(Quaternion current, Quaternion target, ref Vector3 currentVelocity,
        float smoothTime)
    {
        var currentEuler = current.eulerAngles;
        var targetEuler = target.eulerAngles;

        // Handle angle wrapping
        for (var i = 0; i < 3; i++)
        {
            var diff = targetEuler[i] - currentEuler[i];
            if (diff > 180f) targetEuler[i] -= 360f;
            else if (diff < -180f) targetEuler[i] += 360f;
        }

        var smoothedEuler = Vector3.SmoothDamp(currentEuler, targetEuler, ref currentVelocity, smoothTime);
        return Quaternion.Euler(smoothedEuler);
    }
}
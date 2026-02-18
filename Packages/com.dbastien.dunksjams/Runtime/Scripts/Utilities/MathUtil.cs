using UnityEngine;

// MathUtil - specialized math utilities not suitable for extension methods
// Note: MathUtils name is taken by UnityEditor.MathUtils
public static class MathUtil
{
    public static float TriangleArea(Vector2 A, Vector2 B, Vector2 C) =>
        Vector3.Cross(A - B, A - C).z.Abs() / 2;

    public static Vector2 LineIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
    {
        float a1 = B.y - A.y;
        float b1 = A.x - B.x;
        float c1 = a1 * A.x + b1 * A.y;

        float a2 = D.y - C.y;
        float b2 = C.x - D.x;
        float c2 = a2 * C.x + b2 * C.y;

        float d = a1 * b2 - a2 * b1;

        float x = (b2 * c1 - b1 * c2) / d;
        float y = (a1 * c2 - a2 * c1) / d;

        return new Vector2(x, y);
    }

    // Lerp overloads with unclamped behavior
    public static float Lerp(float f1, float f2, float t) => Mathf.LerpUnclamped(f1, f2, t);
    public static float Lerp(ref float f1, float f2, float t) => f1 = Lerp(f1, f2, t);

    public static Vector2 Lerp(Vector2 f1, Vector2 f2, float t) => Vector2.LerpUnclamped(f1, f2, t);
    public static Vector2 Lerp(ref Vector2 f1, Vector2 f2, float t) => f1 = Lerp(f1, f2, t);

    public static Vector3 Lerp(Vector3 f1, Vector3 f2, float t) => Vector3.LerpUnclamped(f1, f2, t);
    public static Vector3 Lerp(ref Vector3 f1, Vector3 f2, float t) => f1 = Lerp(f1, f2, t);

    public static Color Lerp(Color f1, Color f2, float t) => Color.LerpUnclamped(f1, f2, t);
    public static Color Lerp(ref Color f1, Color f2, float t) => f1 = Lerp(f1, f2, t);

    // Lerp with speed & deltaTime
    public static float Lerp(float current, float target, float speed, float deltaTime) =>
        Mathf.Lerp(current, target, GetLerpT(speed, deltaTime));

    public static float Lerp(ref float current, float target, float speed, float deltaTime) =>
        current = Lerp(current, target, speed, deltaTime);

    public static Vector2 Lerp(Vector2 current, Vector2 target, float speed, float deltaTime) =>
        Vector2.Lerp(current, target, GetLerpT(speed, deltaTime));

    public static Vector2 Lerp(ref Vector2 current, Vector2 target, float speed, float deltaTime) =>
        current = Lerp(current, target, speed, deltaTime);

    public static Vector3 Lerp(Vector3 current, Vector3 target, float speed, float deltaTime) =>
        Vector3.Lerp(current, target, GetLerpT(speed, deltaTime));

    public static Vector3 Lerp(ref Vector3 current, Vector3 target, float speed, float deltaTime) =>
        current = Lerp(current, target, speed, deltaTime);

    // SmoothDamp overloads
    public static float SmoothDamp
        (float current, float target, float speed, ref float derivative, float deltaTime, float maxSpeed) =>
        Mathf.SmoothDamp(current, target, ref derivative, .5f / speed, maxSpeed, deltaTime);

    public static float SmoothDamp(float current, float target, float speed, ref float derivative, float deltaTime) =>
        Mathf.SmoothDamp(current, target, ref derivative, .5f / speed, Mathf.Infinity, deltaTime);

    public static float SmoothDamp(float current, float target, float speed, ref float derivative) =>
        SmoothDamp(current, target, speed, ref derivative, Time.deltaTime);

    public static float SmoothDamp
        (ref float current, float target, float speed, ref float derivative, float deltaTime, float maxSpeed) =>
        current = SmoothDamp(current, target, speed, ref derivative, deltaTime, maxSpeed);

    public static float SmoothDamp
        (ref float current, float target, float speed, ref float derivative, float deltaTime) =>
        current = SmoothDamp(current, target, speed, ref derivative, deltaTime);

    public static float SmoothDamp(ref float current, float target, float speed, ref float derivative) =>
        current = SmoothDamp(current, target, speed, ref derivative, Time.deltaTime);

    public static Vector2 SmoothDamp
        (Vector2 current, Vector2 target, float speed, ref Vector2 derivative, float deltaTime) =>
        Vector2.SmoothDamp(current, target, ref derivative, .5f / speed, Mathf.Infinity, deltaTime);

    public static Vector2 SmoothDamp(Vector2 current, Vector2 target, float speed, ref Vector2 derivative) =>
        SmoothDamp(current, target, speed, ref derivative, Time.deltaTime);

    public static Vector2 SmoothDamp
        (ref Vector2 current, Vector2 target, float speed, ref Vector2 derivative, float deltaTime) =>
        current = SmoothDamp(current, target, speed, ref derivative, deltaTime);

    public static Vector2 SmoothDamp(ref Vector2 current, Vector2 target, float speed, ref Vector2 derivative) =>
        current = SmoothDamp(current, target, speed, ref derivative, Time.deltaTime);

    public static Vector3 SmoothDamp
        (Vector3 current, Vector3 target, float speed, ref Vector3 derivative, float deltaTime) =>
        Vector3.SmoothDamp(current, target, ref derivative, .5f / speed, Mathf.Infinity, deltaTime);

    public static Vector3 SmoothDamp(Vector3 current, Vector3 target, float speed, ref Vector3 derivative) =>
        SmoothDamp(current, target, speed, ref derivative, Time.deltaTime);

    public static Vector3 SmoothDamp
        (ref Vector3 current, Vector3 target, float speed, ref Vector3 derivative, float deltaTime) =>
        current = SmoothDamp(current, target, speed, ref derivative, deltaTime);

    public static Vector3 SmoothDamp(ref Vector3 current, Vector3 target, float speed, ref Vector3 derivative) =>
        current = SmoothDamp(current, target, speed, ref derivative, Time.deltaTime);

    // Lerp T calculation
    public static float GetLerpT(float lerpSpeed, float deltaTime) => 1 - Mathf.Exp(-lerpSpeed * 2f * deltaTime);
    public static float GetLerpT(float lerpSpeed) => GetLerpT(lerpSpeed, Time.deltaTime);
}
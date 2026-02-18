using System;
using System.Linq;
using UnityEngine;

public static class FloatExtensions
{
    public static float Abs(this float f) => MathF.Abs(f);
    public static bool IsNaN(this float f) => float.IsNaN(f);
    public static float Remap01(this float x, float min, float max) => (x - min) / (max - min);

    public static float Remap(this float x, float minIn, float maxIn, float minOut, float maxOut) =>
        minOut + (maxOut - minOut) * x.Remap01(minIn, maxIn);

    public static void NormalizeHalfMatrix(this float[] m)
    {
        float s = m[0] + 2 * m.Skip(1).Sum();
        for (var i = 0; i < m.Length; ++i) m[i] /= s;
    }

    public static void NormalizeMatrix(this float[] m)
    {
        float s = m.Sum();
        for (var i = 0; i < m.Length; ++i) m[i] /= s;
    }

    public static float[] ExpandHalfMatrixToFull(this float[] m)
    {
        int lO = m.Length * 2 - 1, cO = lO / 2;
        var res = new float[lO];
        for (var i = 1; i < m.Length; ++i) res[cO - i] = res[cO + i] = m[i];
        res[cO] = m[0];
        return res;
    }

    public static bool Approximately(this float f, float other) => MathF.Abs(f - other) < 0.0001f;

    public static float DistanceTo(this float f1, float f2) => MathF.Abs(f1 - f2);
    public static float Sign(this float f) => f == 0 ? 0 : MathF.Sign(f);
    public static float Clamp(this float f, float f0, float f1) => Mathf.Clamp(f, f0, f1);
    public static float Clamp01(this float f) => Mathf.Clamp(f, 0, 1);
    public static float Pow(this float f, float pow) => MathF.Pow(f, pow);
    public static float Round(this float f) => MathF.Round(f);
    public static float Ceil(this float f) => MathF.Ceiling(f);
    public static float Floor(this float f) => MathF.Floor(f);
    public static int RoundToInt(this float f) => (int)MathF.Round(f);
    public static int CeilToInt(this float f) => (int)MathF.Ceiling(f);
    public static int FloorToInt(this float f) => (int)MathF.Floor(f);
    public static int ToInt(this float f) => (int)f;
    public static float Sqrt(this float f) => MathF.Sqrt(f);
    public static float Max(this float f, float ff) => MathF.Max(f, ff);
    public static float Min(this float f, float ff) => MathF.Min(f, ff);
    public static float ClampMin(this float f, float limitMin) => MathF.Max(f, limitMin);
    public static float ClampMax(this float f, float limitMax) => MathF.Min(f, limitMax);

    public static float Loop(this float f, float boundMin, float boundMax)
    {
        while (f < boundMin) f += boundMax - boundMin;
        while (f > boundMax) f -= boundMax - boundMin;
        return f;
    }

    public static float Loop(this float f, float boundMax) => f.Loop(0, boundMax);

    public static float PingPong
        (this float f, float boundMin, float boundMax) =>
        boundMin + MathF.Abs((f - boundMin) % (2 * (boundMax - boundMin)) - (boundMax - boundMin));

    public static float PingPong(this float f, float boundMax) => f.PingPong(0, boundMax);

    public static float Smoothstep(this float f)
    {
        float t = f.Clamp01();
        return t * t * (3 - 2 * t);
    }

    public static bool Approx(this float f1, float f2) => f1.Approximately(f2);
    public static bool IsInRange(this float i, float a, float b) => i >= a && i <= b;
}
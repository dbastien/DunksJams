using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class Rand
{
    private static ulong _state0 = (ulong)Environment.TickCount;

    private static ulong _state1 = 0x5DEECE66DL;
//    static bool _useSeed;

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static void SetSeed(int seed)
    // {
    //     _state0 = (ulong)seed;
    //     _state1 = _state0 ^ 0x5DEECE66DL;
    //     _useSeed = true;
    // }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static void ClearSeed() => _useSeed = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong NextRaw()
    {
        ulong s1 = _state0;
        ulong s0 = _state1;
        _state0 = s0;
        s1 ^= s1 << 23;
        _state1 = s1 ^ s0 ^ (s1 >> 17) ^ (s0 >> 26);
        return _state1 + s0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float NextFloat() => (float)((NextRaw() >> 40) * (1.0 / (1 << 24)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Bool() => NextFloat() > 0.5f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool BoolWithChance(float chance) => NextFloat() < chance;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Float() => NextFloat();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Radial() => MathF.Sqrt(Float());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FloatExclusive() => NextFloat() * (1f - Mathf.Epsilon);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FloatRanged(float min, float max) => min + (max - min) * NextFloat();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IntRanged(int min, int max) => min + (int)((max - min) * NextFloat());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Degree() => FloatRanged(0f, 360f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Rad() => FloatRanged(0f, MathConsts.Tau);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sign() => Bool() ? 1f : -1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SignInt() => Bool() ? 1 : -1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Vector2() => new(NextFloat(), NextFloat());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Vector3() => new(NextFloat(), NextFloat(), NextFloat());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Color() => new(NextFloat(), NextFloat(), NextFloat());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ColorWithAlpha() => new(NextFloat(), NextFloat(), NextFloat(), NextFloat());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Element<T>(T[] array) => array.Length == 0 ? default : array[IntRanged(0, array.Length)];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Element<T>(IList<T> list) => list.Count == 0 ? default : list[IntRanged(0, list.Count)];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Shuffle<T>(IList<T> list)
    {
        // fisher-yates
        for (int i = list.Count - 1; i > 0; --i)
        {
            int j = IntRanged(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Shuffle<T>(Span<T> span)
    {
        // fisher-yates
        for (int i = span.Length - 1; i > 0; --i)
        {
            int j = IntRanged(0, i + 1);
            (span[i], span[j]) = (span[j], span[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Gaussian()
    {
        float u1 = FloatExclusive();
        float u2 = Float();
        return MathF.Sqrt(-2f * MathF.Log(u1)) * MathF.Sin(MathConsts.Tau * u2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GaussianRanged(float mean, float stdDev) => mean + Gaussian() * stdDev;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LogNormal(float mean = 0f, float stdDev = 1f) =>
        MathF.Exp(Gaussian() * stdDev + mean);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cauchy(float x0 = 0f, float gamma = 1f)
    {
        float u = FloatExclusive() - 0.5f;
        return x0 + gamma * MathF.Tan(MathConsts.Tau / 2f * u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float PowerLaw(float exponent) => MathF.Pow(1f - FloatExclusive(), -1f / exponent);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Poisson(float lambda)
    {
        var k = 0;
        float p = 1f, expLambda = MathF.Exp(-lambda);
        while (p > expLambda)
        {
            p *= Float();
            ++k;
        }

        return k - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Beta(float alpha, float beta)
    {
        float x = MathF.Pow(Float(), 1f / alpha);
        float y = MathF.Pow(Float(), 1f / beta);
        return x / (x + y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Triangular(float min, float max, float mode)
    {
        float u = Float();
        return u < (mode - min) / (max - min)
            ? min + MathF.Sqrt(u * (max - min) * (mode - min))
            : max - MathF.Sqrt((1f - u) * (max - min) * (max - mode));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 PointInBounds(Bounds bounds) =>
        new(FloatRanged(bounds.min.x, bounds.max.x),
            FloatRanged(bounds.min.y, bounds.max.y),
            FloatRanged(bounds.min.z, bounds.max.z));
}
using System;
using System.Collections.Generic;
using UnityEngine;
using R = UnityEngine.Random;

public static class Rand
{
    public static bool Bool() => R.value > 0.5f;
    public static bool BoolWithChance(float chance) => R.value < chance;
    public static float Float() => R.value;
    public static float Radial() => Mathf.Sqrt(Float());

    //[0,1)
    public static float FloatExclusive() => R.value * (1f - MathConsts.ZeroTolerance);

    public static float Degree() => R.Range(0f, 360f);
    public static float Radian() => R.Range(0f, MathConsts.Tau);
    public static float Rad() => R.Range(0f, MathConsts.Tau);
    public static float Sign() => R.value > 0.5f ? 1f : -1f;
    public static int SignInt() => R.value > 0.5f ? 1 : -1;

    public static Vector2 Vector2() => new(R.value, R.value);
    public static Vector3 Vector3() => new(R.value, R.value, R.value);
    public static Vector4 Vector4() => new(R.value, R.value, R.value, R.value);
    public static Color Color() => new(R.value, R.value, R.value);
    public static Color ColorWithAlpha() => new(R.value, R.value, R.value, R.value);

    public static int SignWeighted(float weight) => R.value < weight ? 1 : -1;

    public static Vector2 Vector2Ranged(Vector2 min, Vector2 max) =>
        new(R.Range(min.x, max.x), R.Range(min.y, max.y));

    public static Vector3 Vector3Ranged(Vector3 min, Vector3 max) =>
        new(R.Range(min.x, max.x), R.Range(min.y, max.y), R.Range(min.z, max.z));

    public static Vector4 Vector4Ranged(Vector4 min, Vector4 max) =>
        new(R.Range(min.x, max.x), R.Range(min.y, max.y), R.Range(min.z, max.z), R.Range(min.w, max.w));

    public static Vector2Int Vector2IntRanged(Vector2Int min, Vector2Int max) =>
        new(R.Range(min.x, max.x + 1), R.Range(min.y, max.y + 1));

    public static Vector3Int Vector3IntRanged(Vector3Int min, Vector3Int max) =>
        new(R.Range(min.x, max.x + 1), R.Range(min.y, max.y + 1), R.Range(min.z, max.z + 1));

    public static int IntRanged(int min, int max) => R.Range(min, max);
    public static float FloatRanged(float min, float max) => R.Range(min, max);

    public static Color ColorRanged(Color min, Color max) =>
        new(R.Range(min.r, max.r), R.Range(min.g, max.g), R.Range(min.b, max.b), R.Range(min.a, max.a));

    public static Quaternion Quaternion() =>
        UnityEngine.Quaternion.Euler(R.value * 360f, R.value * 360f, R.value * 360f);

    public static Quaternion RotationAroundAxis(Vector3 axis) =>
        UnityEngine.Quaternion.AngleAxis(Degree(), axis.normalized);

    public static T Element<T>(T[] array) => array[R.Range(0, array.Length)];
    public static T Element<T>(List<T> list) => list[R.Range(0, list.Count)];

    public static T WeightedElement<T>(T[] elements, float[] weights)
    {
        float totalWeight = 0f;
        for (int i = 0; i < weights.Length; ++i) totalWeight += weights[i];
        float randomWeight = Float() * totalWeight;
        for (int i = 0; i < weights.Length; ++i)
        {
            if (randomWeight < weights[i]) return elements[i];
            randomWeight -= weights[i];
        }
        return default;
    }

    public static Vector2 PointInCircle(float radius) => R.insideUnitCircle * radius;
    public static Vector3 PointInSphere(float radius) => R.insideUnitSphere * radius;
    public static Vector3 PointOnSphere(float radius) => R.onUnitSphere * radius;

    public static Vector2 Direction2D() =>
        (new Vector2(R.value, R.value) - new Vector2(0.5f, 0.5f)).normalized;

    public static Vector3 Direction3D() =>
        (new Vector3(R.value, R.value, R.value) - new Vector3(0.5f, 0.5f, 0.5f)).normalized;

    public static bool WeightedBool(float trueWeight, float falseWeight) =>
        R.value < trueWeight / (trueWeight + falseWeight);

    public static T EnumValue<T>() where T : Enum
    {
        var values = EnumCache<T>.Values;
        return values.Length > 0 ? values[R.Range(0, values.Length)] : default;
    }

    public static Vector3 PointInBounds(Bounds bounds) =>
        new(R.Range(bounds.min.x, bounds.max.x),
            R.Range(bounds.min.y, bounds.max.y),
            R.Range(bounds.min.z, bounds.max.z));

    public static Vector2 PointInRect(Rect rect) => new(R.Range(rect.xMin, rect.xMax), R.Range(rect.yMin, rect.yMax));

    public static T[] Array<T>(int len, Func<T> generator)
    {
        T[] array = new T[len];
        for (var i = 0; i < len; ++i) array[i] = generator();
        return array;
    }

    //fischer-yates
    public static void Shuffle<T>(IList<T> list)
    {
        for (var i = 0; i < list.Count; ++i)
        {
            int randomIndex = R.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public static void Shuffle<T>(Span<T> span)
    {
        for (var i = 0; i < span.Length; ++i)
        {
            int randomIndex = R.Range(i, span.Length);
            (span[i], span[randomIndex]) = (span[randomIndex], span[i]);
        }
    }

    public static T WeightedElement<T>(Dictionary<T, float> weightedElements)
    {
        float totalWeight = 0;
        foreach (float weight in weightedElements.Values) totalWeight += weight;

        float randomWeight = R.value * totalWeight;
        foreach (KeyValuePair<T, float> element in weightedElements)
        {
            if (randomWeight < element.Value) return element.Key;
            randomWeight -= element.Value;
        }
        return default;
    }

    public static float Gaussian()
    {
        // Box-Muller transform
        float u1 = FloatExclusive();
        float u2 = Float();
        return MathF.Sqrt(-2f * MathF.Log(u1)) * MathF.Sin(MathConsts.Tau * u2);
    }

    public static float GaussianRanged(float mean, float stdDev) => mean + Gaussian() * stdDev;

    public static Vector2 GaussianVector2() => new(Gaussian(), Gaussian());
    public static Vector3 GaussianVector3() => new(Gaussian(), Gaussian(), Gaussian());
    public static Vector4 GaussianVector4() => new(Gaussian(), Gaussian(), Gaussian(), Gaussian());

    public static float LogNormal(float mean = 0f, float stdDev = 1f) =>
        MathF.Exp(Gaussian() * stdDev + mean);

    public static float LogNormalRanged(float mean, float stdDev) => mean + LogNormal(stdDev);

    public static float Cauchy(float x0 = 0f, float gamma = 1f)
    {
        float u = FloatExclusive() - 0.5f;
        return x0 + gamma * MathF.Tan(MathConsts.TauDiv2 * u);
    }

    public static float Exponential(float lambda = 1f) => -MathF.Log(FloatExclusive()) / lambda;

    public static float PowerLaw(float exponent) => MathF.Pow(1f - FloatExclusive(), -1f / exponent);

    public static int Poisson(float lambda)
    {
        int k = 0;
        float p = 1f, expLambda = MathF.Exp(-lambda);
        while (p > expLambda)
        {
            p *= Float();
            k++;
        }
        return k - 1;
    }

    public static float Beta(float alpha, float beta)
    {
        float x = MathF.Pow(Float(), 1f / alpha);
        float y = MathF.Pow(Float(), 1f / beta);
        return x / (x + y);
    }

    public static float Triangular(float min, float max, float mode)
    {
        float u = Float();
        return u < (mode - min) / (max - min)
            ? min + MathF.Sqrt(u * (max - min) * (mode - min))
            : max - MathF.Sqrt((1f - u) * (max - min) * (max - mode));
    }
}
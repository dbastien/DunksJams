using System;
using UnityEngine;

public static class Ease
{
    //todo: consider slapping [MethodImpl(MethodImplOptions.AggressiveInlining)] on at least the simplest ones
    
    public static float SmoothStep(float t) => t * t * (3f - 2f * t);
    public static float SmoothStepC1(float t) => SmoothStep(SmoothStep(t));
    public static float SmootherStep(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);
    public static float SmootherStepC1(float t) => SmootherStep(SmootherStep(t));

    public static float Linear(float t) => t;

    public static float QuadraticIn(float t) => t * t;
    public static float QuadraticOut(float t) => -(t * (t - 2f));
    public static float QuadraticInOut(float t) => DoEaseInOut(t, QuadraticIn, QuadraticOut);

    public static float CubicIn(float t) => t * t * t;
    public static float CubicOut(float t) => 1f + --t * t * t;
    public static float CubicInOut(float t) => DoEaseInOut(t, CubicIn, CubicOut);

    public static float QuarticIn(float t) => t * t * t * t;
    public static float QuarticOut(float t) => 1f - --t * t * t * t;
    public static float QuarticInOut(float t) => DoEaseInOut(t, QuarticIn, QuarticOut);

    public static float QuinticIn(float t) => t * t * t * t * t;
    public static float QuinticOut(float t) => 1f + --t * t * t * t * t;
    public static float QuinticInOut(float t) => DoEaseInOut(t, QuinticIn, QuinticOut);

    public static float SineIn(float t) => 1f - MathF.Cos(t * MathConsts.TauDiv4);
    public static float SineOut(float t) => MathF.Sin(t * MathConsts.TauDiv4);
    public static float SineInOut(float t) => -0.5f * (MathF.Cos(MathConsts.TauDiv2 * t) - 1f);

    public static float SinHalf(float t) => MathF.Sin(t * MathConsts.TauDiv2);
    public static float Square(float t) => (t < 0.5f) ? 0f : 1f;
    public static float Triangle(float t) => Mathf.Abs((t + .5f) * 2f % 2 - 1f);
    public static float Sawtooth(float t) => (t * 2f) % 1;

    public static float ExponentialIn(float t) => t is 0f ? 0f : Mathf.Pow(2f, 10f * (t - 1f));
    public static float ExponentialOut(float t) => t is 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
    public static float ExponentialInOut(float t) => DoEaseInOut(t, ExponentialIn, ExponentialOut);


    public static float CircularIn(float t) => 1f - MathF.Sqrt(1f - t * t);
    public static float CircularOut(float t) => MathF.Sqrt((2f - t) * t);
    public static float CircularInOut(float t) => DoEaseInOut(t, CircularIn, CircularOut);

    public static float BounceEaseIn(float t) => 1f - BounceEaseOut(1f - t);
    public static float BounceEaseOut(float t) => t switch
    {
        < 4f / 11f => (121f * t * t) / 16f,
        < 8f / 11f => ((363f / 40f) * t * t) - ((99f / 10f) * t) + (17f / 5f),
        < 9f / 10f => ((4356f / 361f) * t * t) - ((35442f / 1805f) * t) + (16061f / 1805f),
        _ => ((54f / 5f) * t * t) - ((513f / 25f) * t) + (268f / 25f)
    };
    public static float BounceEaseInOut(float t) => DoEaseInOut(t, BounceEaseIn, BounceEaseOut);

    const float BackOvershoot = 1.70158f;
    public static float BackIn(float t) => t * t * ((BackOvershoot + 1f) * t - BackOvershoot);
    public static float BackOut(float t) => (t - 1f) * (t - 1f) * ((BackOvershoot + 1f) * (t - 1f) + BackOvershoot) + 1f;
    public static float BackInOut(float t) => DoEaseInOut(t, BackIn, BackOut);

    public static float ElasticIn(float t) => t is 0f or 1f ? t : -MathF.Pow(2f, 10f * (t - 1f)) * MathF.Sin((t - 1.1f) * 5f * Mathf.PI);
    public static float ElasticOut(float t) => t is 0f or 1f ? t : MathF.Pow(2f, -10f * t) * MathF.Sin((t - 0.1f) * 5f * Mathf.PI) + 1f;
    public static float ElasticInOut(float t)
    {
        if (t is 0f or 1f) return t;
        t *= 2f;
        if (t < 1f) return -0.5f * MathF.Pow(2f, 10f * (t - 1f)) * MathF.Sin((t - 1.1f) * 5f * Mathf.PI);
        t -= 1f;
        return 0.5f * MathF.Pow(2f, -10f * t) * MathF.Sin((t - 0.1f) * 5f * Mathf.PI) + 1f;
    }

    public static float DoEaseInOut(float t, Func<float, float> easeIn, Func<float, float> easeOut) =>
        t < 0.5f ? 0.5f * easeIn(t * 2f) : 0.5f * easeOut(t * 2f - 1f) + 0.5f;

    public static Func<float, float> GetEasingFunction(EaseType easeType) => easeType switch
    {
        EaseType.Linear => Linear,
        EaseType.SmoothStep => SmoothStep,
        EaseType.SmoothStepC1 => SmoothStepC1,
        EaseType.SmootherStep => SmootherStep,
        EaseType.SmootherStepC1 => SmootherStepC1,
        EaseType.QuadraticIn => QuadraticIn,
        EaseType.QuadraticOut => QuadraticOut,
        EaseType.QuadraticInOut => QuadraticInOut,
        EaseType.CubicIn => CubicIn,
        EaseType.CubicOut => CubicOut,
        EaseType.CubicInOut => CubicInOut,
        EaseType.QuarticIn => QuarticIn,
        EaseType.QuarticOut => QuarticOut,
        EaseType.QuarticInOut => QuarticInOut,
        EaseType.QuinticIn => QuinticIn,
        EaseType.QuinticOut => QuinticOut,
        EaseType.QuinticInOut => QuinticInOut,
        EaseType.SineIn => SineIn,
        EaseType.SineOut => SineOut,
        EaseType.SineInOut => SineInOut,
        EaseType.ExponentialIn => ExponentialIn,
        EaseType.ExponentialOut => ExponentialOut,
        EaseType.ExponentialInOut => ExponentialInOut,
        EaseType.CircularIn => CircularIn,
        EaseType.CircularOut => CircularOut,
        EaseType.CircularInOut => CircularInOut,
        EaseType.BounceEaseIn => BounceEaseIn,
        EaseType.BounceEaseOut => BounceEaseOut,
        EaseType.BounceEaseInOut => BounceEaseInOut,
        EaseType.BackIn => BackIn,
        EaseType.BackOut => BackOut,
        EaseType.BackInOut => BackInOut,
        EaseType.ElasticIn => ElasticIn,
        EaseType.ElasticOut => ElasticOut,
        EaseType.ElasticInOut => ElasticInOut,
        _ => Linear,
    };
}
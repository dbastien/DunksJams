using System;
using System.Collections.Generic;
using UnityEngine;

public static class FBMNoise
{
    public static float FBM2D(float x, float y, int octaves, float persistence = 0.5f)
    {
        float frequency = 1f, amplitude = 1f, total = 0f, maxAmplitude = 0f;

        for (var i = 0; i < octaves; ++i)
        {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            maxAmplitude += amplitude;
            frequency *= 2f;
            amplitude *= persistence;
        }

        return total / maxAmplitude; // [0, 1]
    }

    public static float FBM3D(float x, float y, float z, int octaves, float persistence = 0.5f)
    {
        float frequency = 1f, amplitude = 1f, total = 0f, maxAmplitude = 0f;

        for (var i = 0; i < octaves; ++i)
        {
            total += PerlinNoise.Perlin3D(x * frequency, y * frequency, z * frequency) * amplitude;
            maxAmplitude += amplitude;
            frequency *= 2f;
            amplitude *= persistence;
        }

        return total / maxAmplitude; // [0, 1]
    }

    public static float FBM2D(float x, float y, int octaves, float persistence, Func<float, float, float> baseNoise)
    {
        float frequency = 1f, amplitude = 1f, total = 0f, maxAmplitude = 0f;

        for (var i = 0; i < octaves; ++i)
        {
            total += baseNoise(x * frequency, y * frequency) * amplitude;
            maxAmplitude += amplitude;
            frequency *= 2f;
            amplitude *= persistence;
        }

        return total / maxAmplitude; // Normalize to [0, 1]
    }

    /// <summary>
    /// Perlin fractal noise with lacunarity and gain control, with optional ridged mode.
    /// Ridged noise inverts the absolute value of each octave for ridge-like features.
    /// </summary>
    public static float FBM2D(float x, float y, int octaves, float lacunarity, float gain, bool ridged)
    {
        float value = 0f, freq = 1f, amp = gain;

        for (int o = 0; o < octaves; o++)
        {
            float octaveValue = Mathf.PerlinNoise(x * freq, y * freq);
            freq *= lacunarity;

            if (ridged)
                octaveValue = 1f - Mathf.Abs(octaveValue * 2f - 1f);

            value += octaveValue * amp;
            amp *= gain;
        }

        return value;
    }

    /// <summary>
    /// Perlin fractal noise with per-octave AnimationCurve modifiers.
    /// Each curve modifier is evaluated on the octave value before amplitude scaling.
    /// </summary>
    public static float FBM2D(float x, float y, int octaves, float lacunarity, float gain,
        List<AnimationCurve> curveModifiers)
    {
        float value = 0f, freq = 1f, amp = gain;

        for (int o = 0; o < octaves; o++)
        {
            float octaveValue = Mathf.PerlinNoise(x * freq, y * freq);
            freq *= lacunarity;

            if (curveModifiers != null)
                for (int cm = 0; cm < curveModifiers.Count; cm++)
                    octaveValue = curveModifiers[cm].Evaluate(octaveValue);

            value += octaveValue * amp;
            amp *= gain;
        }

        return value;
    }

    /// <summary>Fast integer power for float base.</summary>
    public static float IntPow(float num, int pow)
    {
        if (pow == 0) return 1f;
        float ans = num;
        for (int i = 1; i < pow; i++) ans *= num;
        return ans;
    }

    /// <summary>Fast integer power for int base.</summary>
    public static int IntPow(int num, int pow)
    {
        if (pow == 0) return 1;
        int ans = num;
        for (int i = 1; i < pow; i++) ans *= num;
        return ans;
    }
}

public class FBMNoiseGenerator : INoiseGenerator
{
    public int Octaves { get; set; } = 3;
    public float Persistence { get; set; } = 0.5f;
    public float Frequency { get; set; } = 1f;

    float _maxAmplitude = -1f;

    float MaxAmplitude
    {
        get
        {
            if (_maxAmplitude < 0)
                _maxAmplitude = ComputeMaxAmplitude();
            return _maxAmplitude;
        }
    }

    float ComputeMaxAmplitude()
    {
        float amplitude = 1f, maxAmplitude = 0f;
        for (var i = 0; i < Octaves; ++i)
        {
            maxAmplitude += amplitude;
            amplitude *= Persistence;
        }

        return maxAmplitude;
    }

    public float GetValue(float x) =>
        FBMNoise.FBM2D(x * Frequency, 0, Octaves, Persistence) / MaxAmplitude;

    public float GetValue(float x, float y) =>
        FBMNoise.FBM2D(x * Frequency, y * Frequency, Octaves, Persistence) / MaxAmplitude;

    public float GetValue(float x, float y, Func<float, float, float> baseNoise) =>
        FBMNoise.FBM2D(x * Frequency, y * Frequency, Octaves, Persistence, baseNoise) / MaxAmplitude;

    public float GetValue(float x, float y, float z) =>
        FBMNoise.FBM3D(x * Frequency, y * Frequency, z * Frequency, Octaves, Persistence) / MaxAmplitude;
}
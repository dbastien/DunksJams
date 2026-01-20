using UnityEngine;

public static class FBMNoise
{
    public static float FBM2D(float x, float y, int octaves, float persistence = 0.5f)
    {
        float frequency = 1f, amplitude = 1f, total = 0f, maxAmplitude = 0f;

        for (int i = 0; i < octaves; ++i)
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

        for (int i = 0; i < octaves; ++i)
        {
            total += PerlinNoise.Perlin3D(x * frequency, y * frequency, z * frequency) * amplitude;
            maxAmplitude += amplitude;
            frequency *= 2f;
            amplitude *= persistence;
        }

        return total / maxAmplitude; // [0, 1]
    }

    public static float FBM2D(float x, float y, int octaves, float persistence, System.Func<float, float, float> baseNoise)
    {
        float frequency = 1f, amplitude = 1f, total = 0f, maxAmplitude = 0f;

        for (int i = 0; i < octaves; ++i)
        {
            total += baseNoise(x * frequency, y * frequency) * amplitude;
            maxAmplitude += amplitude;
            frequency *= 2f;
            amplitude *= persistence;
        }

        return total / maxAmplitude; // Normalize to [0, 1]
    }
}

public class FBMNoiseGenerator : INoiseGenerator
{
    public int Octaves { get; set; } = 3;
    public float Persistence { get; set; } = 0.5f;
    public float Frequency { get; set; } = 1f;

    private float _maxAmplitude = -1f;

    private float MaxAmplitude
    {
        get
        {
            if (_maxAmplitude < 0)
                _maxAmplitude = ComputeMaxAmplitude();
            return _maxAmplitude;
        }
    }

    private float ComputeMaxAmplitude()
    {
        float amplitude = 1f, maxAmplitude = 0f;
        for (int i = 0; i < Octaves; ++i)
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

    public float GetValue(float x, float y, System.Func<float, float, float> baseNoise) =>
        FBMNoise.FBM2D(x * Frequency, y * Frequency, Octaves, Persistence, baseNoise) / MaxAmplitude;

    public float GetValue(float x, float y, float z) =>
        FBMNoise.FBM3D(x * Frequency, y * Frequency, z * Frequency, Octaves, Persistence) / MaxAmplitude;
}
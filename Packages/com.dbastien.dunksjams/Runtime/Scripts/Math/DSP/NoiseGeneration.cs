using UnityEngine;
using Random = System.Random;

public static class NoiseGeneration
{
    private static readonly Random random = new Random();

    public static float[] WhiteNoise(int length, float amplitude = 1f)
    {
        float[] noise = new float[length];
        for (int i = 0; i < length; i++)
        {
            noise[i] = ((float)random.NextDouble() * 2f - 1f) * amplitude;
        }
        return noise;
    }

    public static float[] PinkNoise(int length, float amplitude = 1f)
    {
        float[] noise = new float[length];
        float[] b = new float[7]; // Pink noise filter states

        for (int i = 0; i < length; i++)
        {
            float white = (float)random.NextDouble() * 2f - 1f;

            // Pink noise filter
            b[0] = 0.99886f * b[0] + white * 0.0555179f;
            b[1] = 0.99332f * b[1] + white * 0.0750759f;
            b[2] = 0.96900f * b[2] + white * 0.1538520f;
            b[3] = 0.86650f * b[3] + white * 0.3104856f;
            b[4] = 0.55000f * b[4] + white * 0.5329522f;
            b[5] = -0.7616f * b[5] - white * 0.0168980f;
            float pink = b[0] + b[1] + b[2] + b[3] + b[4] + b[5] + b[6] + white * 0.5362f;
            b[6] = white * 0.115926f;

            noise[i] = pink * amplitude;
        }

        return noise;
    }

    public static float[] BrownNoise(int length, float amplitude = 1f)
    {
        float[] noise = new float[length];
        float lastValue = 0f;

        for (int i = 0; i < length; i++)
        {
            float white = (float)random.NextDouble() * 2f - 1f;
            lastValue += white * 0.1f; // Low-pass filter
            lastValue = Mathf.Clamp(lastValue, -1f, 1f); // Prevent drift
            noise[i] = lastValue * amplitude;
        }

        return noise;
    }

    public static float[] PerlinNoise(int length, float frequency = 0.01f, float amplitude = 1f)
    {
        float[] noise = new float[length];

        for (int i = 0; i < length; i++)
        {
            float x = i * frequency;
            noise[i] = Mathf.PerlinNoise(x, 0f) * 2f - 1f; // Convert to -1 to 1 range
            noise[i] *= amplitude;
        }

        return noise;
    }
}
using UnityEngine;

public static class PerlinNoise
{
    public static float Perlin2D(float x, float y) => Mathf.PerlinNoise(x, y);

    public static float Perlin3D(float x, float y, float z)
    {
        var xy = Mathf.PerlinNoise(x, y);
        var yz = Mathf.PerlinNoise(y, z);
        var zx = Mathf.PerlinNoise(z, x);
        return (xy + yz + zx) / 3f;
    }
}

public class PerlinNoiseGenerator : INoiseGenerator
{
    public float Frequency { get; set; } = 1f;
    public float GetValue(float x) => PerlinNoise.Perlin2D(x * Frequency, 0);
    public float GetValue(float x, float y) => PerlinNoise.Perlin2D(x * Frequency, y * Frequency);

    public float GetValue(float x, float y, float z) =>
        PerlinNoise.Perlin3D(x * Frequency, y * Frequency, z * Frequency);
}
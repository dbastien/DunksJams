using Unity.Mathematics;

public static class SimplexNoise
{
    public static float Simplex2D(float x, float y) => (noise.snoise(new float2(x, y)) + 1f) * 0.5f;
    public static float Simplex3D(float x, float y, float z) => (noise.snoise(new float3(x, y, z)) + 1f) * 0.5f;

    public static float Tiled2D(float x, float y, float tileSize) =>
        Simplex2D(x % tileSize / tileSize, y % tileSize / tileSize);

    public static float Tiled3D(float x, float y, float z, float tileSize) => Simplex3D(x % tileSize / tileSize,
        y % tileSize / tileSize, z % tileSize / tileSize);
}

public class SimplexNoiseGenerator : INoiseGenerator
{
    public float Frequency { get; set; } = 1f;

    public float GetValue(float x) => SimplexNoise.Simplex2D(x * Frequency, 0);
    public float GetValue(float x, float y) => SimplexNoise.Simplex2D(x * Frequency, y * Frequency);

    public float GetValue(float x, float y, float z) =>
        SimplexNoise.Simplex3D(x * Frequency, y * Frequency, z * Frequency);
}
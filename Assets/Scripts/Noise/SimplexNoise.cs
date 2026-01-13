using Unity.Mathematics;

public static class SimplexNoise
{
    /// <summary>2D Simplex noise, returns [0, 1].</summary>
    public static float Simplex2D(float x, float y) =>
        (noise.snoise(new float2(x, y)) + 1f) * 0.5f;

    /// <summary>3D Simplex noise, returns [0, 1].</summary>
    public static float Simplex3D(float x, float y, float z) =>
        (noise.snoise(new float3(x, y, z)) + 1f) * 0.5f;

    /// <summary>Tileable 2D Simplex noise.</summary>
    public static float Tiled2D(float x, float y, float tileSize)
    {
        float nx = (x % tileSize) / tileSize;
        float ny = (y % tileSize) / tileSize;
        return Simplex2D(nx * tileSize, ny * tileSize);
    }

    /// <summary>Tileable 3D Simplex noise.</summary>
    public static float Tiled3D(float x, float y, float z, float tileSize)
    {
        float nx = (x % tileSize) / tileSize;
        float ny = (y % tileSize) / tileSize;
        float nz = (z % tileSize) / tileSize;
        return Simplex3D(nx * tileSize, ny * tileSize, nz * tileSize);
    }
}

public class SimplexNoiseGenerator : INoiseGenerator
{
    public float Frequency { get; set; } = 1f;

    public float GetValue(float x) => SimplexNoise.Simplex2D(x * Frequency, 0);
    public float GetValue(float x, float y) => SimplexNoise.Simplex2D(x * Frequency, y * Frequency);
    public float GetValue(float x, float y, float z) => SimplexNoise.Simplex3D(x * Frequency, y * Frequency, z * Frequency);
}

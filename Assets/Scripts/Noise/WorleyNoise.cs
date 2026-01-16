using static Unity.Mathematics.math;
using Unity.Mathematics;

public static class WorleyNoise
{
    /// <summary>2D Worley/Cellular noise. Returns F1 (distance to nearest point), [0, ~1].</summary>
    public static float Worley2D(float x, float y)
    {
        // noise.cellular returns float2(F1, F2) - distances to nearest and second-nearest points
        return noise.cellular(new float2(x, y)).x;
    }

    /// <summary>2D Worley noise with cell count control.</summary>
    public static float Worley2D(float x, float y, int cells)
    {
        return noise.cellular(new float2(x * cells, y * cells)).x;
    }

    /// <summary>3D Worley/Cellular noise. Returns F1 (distance to nearest point).</summary>
    public static float Worley3D(float x, float y, float z)
    {
        return noise.cellular(new float3(x, y, z)).x;
    }
}

public class WorleyNoiseGenerator : INoiseGenerator
{
    public float Frequency { get; set; } = 1f;

    public float GetValue(float x) => WorleyNoise.Worley2D(x * Frequency, 0);
    public float GetValue(float x, float y) => WorleyNoise.Worley2D(x * Frequency, y * Frequency);
    public float GetValue(float x, float y, float z) => WorleyNoise.Worley3D(x * Frequency, y * Frequency, z * Frequency);
}

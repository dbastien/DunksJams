using Unity.Mathematics;    

public static class WorleyNoise
{
    public static float Worley2D(float x, float y) => noise.cellular(new float2(x, y)).x;
    public static float Worley2D(float x, float y, int cells) => noise.cellular(new float2(x * cells, y * cells)).x;
    public static float Worley3D(float x, float y, float z) => noise.cellular(new float3(x, y, z)).x;
}

public class WorleyNoiseGenerator : INoiseGenerator
{
    public float Frequency { get; set; } = 1f;

    public float GetValue(float x) => WorleyNoise.Worley2D(x * Frequency, 0);
    public float GetValue(float x, float y) => WorleyNoise.Worley2D(x * Frequency, y * Frequency);
    public float GetValue(float x, float y, float z) => WorleyNoise.Worley3D(x * Frequency, y * Frequency, z * Frequency);
}

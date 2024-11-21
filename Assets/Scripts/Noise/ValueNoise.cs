public static class ValueNoise
{
    public static float Normalized1D(uint x, uint seed = 0) => ToUnitFloat(Hash(x, seed));

    public static float Normalized2D(uint x, uint y, uint seed = 0) => ToUnitFloat(Hash(x + 198491317u * y, seed));

    public static float Normalized3D(uint x, uint y, uint z, uint seed = 0) =>
        ToUnitFloat(Hash(x + 198491317u * y + 6542989u * z, seed));

    private static uint Hash(uint input, uint seed)
    {
        unchecked
        {
            uint h = input * 0xB515781D;
            h ^= h >> 8;
            h += 0x68DA4024 + seed;
            h ^= h << 8;
            h *= 0x1B56C8D9;
            return h ^ (h >> 8);
        }
    }

    private static float ToUnitFloat(uint value) => value / (float)uint.MaxValue;
}

public class ValueNoiseGenerator : INoiseGenerator
{
    public uint Seed { get; set; } = 0;

    public float GetValue(float x) => ValueNoise.Normalized1D((uint)x, Seed);

    public float GetValue(float x, float y) => ValueNoise.Normalized2D((uint)x, (uint)y, Seed);

    public float GetValue(float x, float y, float z) => ValueNoise.Normalized3D((uint)x, (uint)y, (uint)z, Seed);
}
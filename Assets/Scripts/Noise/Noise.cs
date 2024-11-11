using System.Runtime.CompilerServices;
using UnityEngine;

public static class ValueNoise
{
    const uint Hash1 = 0xB515781D; // 3039394381
    const uint Hash2 = 0x68DA4024; // 1759714724
    const uint Hash3 = 0x1B56C8D9; // 458671337
    const uint PrimeX = 198491317;
    const uint PrimeY = 6542989;
    const uint PrimeZ = 374761393;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Value1D(uint x, uint seed = 0) => Hash(x, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Value2D(uint x, uint y, uint seed = 0) => Hash(x + PrimeX * y, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Value3D(uint x, uint y, uint z, uint seed = 0) => Hash(x + PrimeX * y + PrimeY * z, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Normalized1D(uint x, uint seed = 0) => ToUnitFloat(Value1D(x, seed));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Normalized2D(uint x, uint y, uint seed = 0) => ToUnitFloat(Value2D(x, y, seed));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Normalized3D(uint x, uint y, uint z, uint seed = 0) => ToUnitFloat(Value3D(x, y, z, seed));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint Hash(uint input, uint seed)
    {
        unchecked
        {
            uint h = input * Hash1;
            h ^= h >> 8;
            h += Hash2 + seed;
            h ^= h << 8;
            h *= Hash3;
            return h ^ (h >> 8);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float ToUnitFloat(uint value) => value / (float)uint.MaxValue;
}

public static class PerlinNoise
{
    public static float Perlin2D(float x, float y) => Mathf.PerlinNoise(x, y);

    public static float Perlin3D(float x, float y, float z) =>
        (Mathf.PerlinNoise(x, y) + Mathf.PerlinNoise(y, z) + Mathf.PerlinNoise(z, x)) / 3f;

    public static float TiledPerlin2D(float x, float y, float tileSize)
    {
        float nx = x % tileSize / tileSize;
        float ny = y % tileSize / tileSize;
        return Mathf.PerlinNoise(nx * tileSize, ny * tileSize);
    }
}

public static class SimplexNoise
{
    static readonly int[] Perm = new int[512];
    static readonly int[] Grad3 = {
        1,1,0, -1,1,0, 1,-1,0, -1,-1,0,
        1,0,1, -1,0,1, 1,0,-1, -1,0,-1,
        0,1,1, 0,-1,1, 0,1,-1, 0,-1,-1
    };
    
    public static readonly float F2 = 0.5f * (Mathf.Sqrt(3f) - 1f);
    public static readonly float G2 = (3f - Mathf.Sqrt(3f)) / 6f;

    static SimplexNoise()
    {
        var p = new int[256];
        for (int i = 0; i < 256; ++i) p[i] = i;
        var rand = new System.Random();
        for (int i = 255; i >= 0; --i)
        {
            int j = rand.Next(i + 1);
            (p[i], p[j]) = (p[j], p[i]);
            Perm[i] = Perm[i + 256] = p[i];
        }
    }

    public static float Simplex2D(float xin, float yin)
    {
        float s = (xin + yin) * F2;
        int i = Mathf.FloorToInt(xin + s);
        int j = Mathf.FloorToInt(yin + s);

        float t = (i + j) * G2;
        float X0 = i - t;
        float Y0 = j - t;
        float x0 = xin - X0;
        float y0 = yin - Y0;

        int i1 = (x0 > y0) ? 1 : 0;
        int j1 = (x0 > y0) ? 0 : 1;

        float x1 = x0 - i1 + G2;
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1f + 2f * G2;
        float y2 = y0 - 1f + 2f * G2;

        int ii = i & 255;
        int jj = j & 255;

        float n0 = GetNoiseContribution(ii, jj, x0, y0);
        float n1 = GetNoiseContribution(ii + i1, jj + j1, x1, y1);
        float n2 = GetNoiseContribution(ii + 1, jj + 1, x2, y2);

        return 70f * (n0 + n1 + n2);
    }

    public static float Simplex3D(float xin, float yin, float zin)
    {
        const float F3 = 1f / 3f;
        const float G3 = 1f / 6f;

        float s = (xin + yin + zin) * F3;
        int i = Mathf.FloorToInt(xin + s);
        int j = Mathf.FloorToInt(yin + s);
        int k = Mathf.FloorToInt(zin + s);

        float t = (i + j + k) * G3;
        float X0 = i - t;
        float Y0 = j - t;
        float Z0 = k - t;
        float x0 = xin - X0;
        float y0 = yin - Y0;
        float z0 = zin - Z0;

        (int i1, int j1, int k1) = (x0 >= y0) ? ((y0 >= z0) ? (1, 0, 0) : (0, 0, 1)) : ((x0 >= z0) ? (1, 0, 0) : (0, 1, 0));
        (int i2, int j2, int k2) = (x0 >= y0) ? ((y0 >= z0) ? (1, 1, 0) : (0, 1, 1)) : ((x0 >= z0) ? (1, 0, 1) : (0, 1, 1));

        float x1 = x0 - i1 + G3;
        float y1 = y0 - j1 + G3;
        float z1 = z0 - k1 + G3;
        float x2 = x0 - i2 + 2f * G3;
        float y2 = y0 - j2 + 2f * G3;
        float z2 = z0 - k2 + 2f * G3;
        float x3 = x0 - 1f + 3f * G3;
        float y3 = y0 - 1f + 3f * G3;
        float z3 = z0 - 1f + 3f * G3;

        int ii = i & 255;
        int jj = j & 255;
        int kk = k & 255;

        float n0 = GetNoiseContribution(ii, jj, kk, x0, y0, z0);
        float n1 = GetNoiseContribution(ii + i1, jj + j1, kk + k1, x1, y1, z1);
        float n2 = GetNoiseContribution(ii + i2, jj + j2, kk + k2, x2, y2, z2);
        float n3 = GetNoiseContribution(ii + 1, jj + 1, kk + 1, x3, y3, z3);

        return 32f * (n0 + n1 + n2 + n3);
    }

    static float GetNoiseContribution(int i, int j, float x, float y)
    {
        float t = 0.5f - x * x - y * y;
        if (t < 0) return 0f;
        t *= t;
        int gi = Perm[i + Perm[j]] % 12;
        return t * t * (Grad3[gi * 3] * x + Grad3[gi * 3 + 1] * y);
    }

    static float GetNoiseContribution(int i, int j, int k, float x, float y, float z)
    {
        float t = 0.6f - x * x - y * y - z * z;
        if (t < 0) return 0f;
        t *= t;
        int gi = Perm[i + Perm[j + Perm[k]]] % 12;
        return t * t * (Grad3[gi * 3] * x + Grad3[gi * 3 + 1] * y + Grad3[gi * 3 + 2] * z);
    }
}

public static class WorleyNoise
{
    public static float Worley2D(float x, float y, int cells = 5)
    {
        float minDist = float.MaxValue;
        for (int i = 0; i < cells; ++i)
        {
            for (int j = 0; j < cells; ++j)
            {
                //todo: we could save the increment and just do +=
                float cx = i / (float)cells;
                float cy = j / (float)cells;
                float dist = Vector2.Distance(new(x, y), new(cx, cy));
                minDist = Mathf.Min(minDist, dist);
            }
        }
        return minDist;
    }
}

public static class FractalBrownianMotion
{
    public static float FBM2D(float x, float y, int octaves, float persistence = 0.5f)
    {
        float frequency = 1f, amplitude = 1f, total = 0f;
        for (int i = 0; i < octaves; ++i)
        {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            amplitude *= persistence;
            frequency *= 2f;
        }
        return total;
    }

    public static float FBM3D(float x, float y, float z, int octaves, float persistence = 0.5f)
    {
        float freq = 1f, amp = 1f, total = 0f;
        for (int i = 0; i < octaves; ++i)
        {
            total += PerlinNoise.Perlin3D(x * freq, y * freq, z * freq) * amp;
            amp *= persistence;
            freq *= 2f;
        }
        return total;
    }

    public static float RidgedFBM2D(float x, float y, int octaves, float gain = 2f, float lacunarity = 2f)
    {
        float freq = 1f, amp = 0.5f, total = 0f;
        for (int i = 0; i < octaves; ++i)
        {
            float noise = 1f - Mathf.Abs(Mathf.PerlinNoise(x * freq, y * freq) * 2f - 1f);
            total += noise * amp;
            amp *= gain;
            freq *= lacunarity;
        }
        return total;
    }
}

public static class DomainWarping
{
    public static float Warp2D(float x, float y, float frequency, int octaves)
    {
        float warpX = FractalBrownianMotion.FBM2D(x, y, octaves);
        float warpY = FractalBrownianMotion.FBM2D(x + 5.2f, y + 1.3f, octaves);
        return PerlinNoise.Perlin2D(x + warpX * frequency, y + warpY * frequency);
    }
}
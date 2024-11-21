using UnityEngine;

public static class SimplexNoise
{
    private static readonly int[] Perm = new int[512];
    private static readonly int[] Grad3 = {
        1,1,0, -1,1,0, 1,-1,0, -1,-1,0,
        1,0,1, -1,0,1, 1,0,-1, -1,0,-1,
        0,1,1, 0,-1,1, 0,1,-1, 0,-1,-1
    };

    static SimplexNoise() => InitializePerm();

    private static void InitializePerm()
    {
        var p = new int[256];
        for (int i = 0; i < 256; ++i) p[i] = i;
        Rand.Shuffle(p);
        for (int i = 0; i < 256; ++i) Perm[i] = Perm[i + 256] = p[i];
    }

    private static readonly float F2 = 0.5f * (Mathf.Sqrt(3f) - 1f);
    private static readonly float G2 = (3f - Mathf.Sqrt(3f)) / 6f;

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

        int i1 = x0 > y0 ? 1 : 0;
        int j1 = x0 > y0 ? 0 : 1;

        float x1 = x0 - i1 + G2;
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1f + 2f * G2;
        float y2 = y0 - 1f + 2f * G2;

        int ii = i & 255;
        int jj = j & 255;

        float n0 = GetNoiseContrib(ii, jj, x0, y0);
        float n1 = GetNoiseContrib(ii + i1, jj + j1, x1, y1);
        float n2 = GetNoiseContrib(ii + 1, jj + 1, x2, y2);

        return (70f * (n0 + n1 + n2)).Remap01(-1f, 1f);
    }

    private const float F3 = 1f / 3f;
    private const float G3 = 1f / 6f;

    public static float Simplex3D(float xin, float yin, float zin)
    {
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

        (int i1, int j1, int k1) = x0 >= y0 ? y0 >= z0 ? (1, 0, 0) : (0, 0, 1) : x0 >= z0 ? (1, 0, 0) : (0, 1, 0);
        (int i2, int j2, int k2) = x0 >= y0 ? y0 >= z0 ? (1, 1, 0) : (0, 1, 1) : x0 >= z0 ? (1, 0, 1) : (0, 1, 1);

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

        float n0 = GetNoiseContrib(ii, jj, kk, x0, y0, z0);
        float n1 = GetNoiseContrib(ii + i1, jj + j1, kk + k1, x1, y1, z1);
        float n2 = GetNoiseContrib(ii + i2, jj + j2, kk + k2, x2, y2, z2);
        float n3 = GetNoiseContrib(ii + 1, jj + 1, kk + 1, x3, y3, z3);

        return (32f * (n0 + n1 + n2 + n3)).Remap01(-1f, 1f);
    }

    private static float GetNoiseContrib(int i, int j, float x, float y)
    {
        float t = 0.5f - (x * x + y * y);
        if (t < 0) return 0f;
        t *= t;
        int gi = Perm[i + Perm[j]] % 12;
        return t * t * (Grad3[gi * 3] * x + Grad3[gi * 3 + 1] * y);
    }

    private static float GetNoiseContrib(int i, int j, int k, float x, float y, float z)
    {
        float t = 0.6f - (x * x + y * y + z * z);
        if (t < 0) return 0f;
        t *= t;
        int gi = Perm[i + Perm[j + Perm[k]]] % 12;
        return t * t * (Grad3[gi * 3] * x + Grad3[gi * 3 + 1] * y + Grad3[gi * 3 + 2] * z);
    }

    public static float Tiled2D(float x, float y, float tileSize)
    {
        float nx = x % tileSize / tileSize;
        float ny = y % tileSize / tileSize;
        return Simplex2D(nx * tileSize, ny * tileSize);
    }

    public static float Tiled3D(float x, float y, float z, float tileSize)
    {
        float nx = x % tileSize / tileSize;
        float ny = y % tileSize / tileSize;
        float nz = z % tileSize / tileSize;
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
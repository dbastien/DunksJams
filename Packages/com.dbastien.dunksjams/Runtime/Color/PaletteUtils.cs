using System.Collections.Generic;
using UnityEngine;

public static class PaletteUtils
{
    /// <summary>Convert a ColorPalette into a 1D Texture2D (width = palette length, height = 1).</summary>
    public static Texture2D PaletteToTexture(ColorPalette palette, bool linear = false)
    {
        Color[] colors = palette?.ToArray() ?? new Color[0];
        int width = Mathf.Max(1, colors.Length);
        var tex = new Texture2D(width, 1, TextureFormat.RGBA32, false, linear)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
        tex.SetPixels(colors);
        tex.Apply(false, false);
        return tex;
    }

    /// <summary>
    /// Generate a flattened 2D LUT texture of size (size*size) x size where each input RGB is mapped to the nearest palette color by perceptual DeltaE.
    /// Use size=16 or size=32 for common LUT sizes. Returns null if palette is null or empty.
    /// </summary>
    public static Texture2D PaletteToLut(ColorPalette palette, int size = 16)
    {
        if (palette == null || palette.Count == 0) return null;
        size = Mathf.Max(2, Mathf.Min(64, size));

        Color[] colors = palette.ToArray();

        // caching key (avoid ValueTuple to be compatible with older Unity runtimes)
        string cacheKey = palette.GetInstanceID() + "_" + size;
        if (_lutCache.TryGetValue(cacheKey, out Texture2D cached)) return cached;

        int width = size * size;
        int height = size;
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        var pixels = new Color[width * height];

        for (var z = 0; z < size; ++z)
        for (var y = 0; y < size; ++y)
        for (var x = 0; x < size; ++x)
        {
            // normalized input color
            float rf = (x + 0.5f) / size;
            float gf = (y + 0.5f) / size;
            float bf = (z + 0.5f) / size;
            var inColor = new Color(rf, gf, bf);

            // find nearest palette color by DeltaE
            Color best = colors[0];
            var bestDist = float.MaxValue;
            for (var i = 0; i < colors.Length; ++i)
            {
                float d = ColorTheory.DeltaE(inColor, colors[i]);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = colors[i];
                }
            }

            int px = x + y * size;
            int py = z;
            int idx = py * width + px;
            pixels[idx] = new Color(best.r, best.g, best.b, 1f);
        }

        tex.SetPixels(pixels);
        tex.Apply(false, true);
        _lutCache[cacheKey] = tex;
        return tex;
    }

    private static readonly Dictionary<string, Texture2D> _lutCache = new();

    /// <summary>
    /// Clear the internal LUT cache. Caller should ensure textures are destroyed if needed.
    /// </summary>
    public static void ClearLutCache() { _lutCache.Clear(); }

    /// <summary>
    /// Remove a specific LUT cache entry for a given palette and size.
    /// </summary>
    public static void RemoveLutFromCache(ColorPalette palette, int size)
    {
        string key = palette.GetInstanceID() + "_" + size;
        if (_lutCache.ContainsKey(key)) _lutCache.Remove(key);
    }
}
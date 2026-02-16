using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight cache for LUT Texture2D pixel arrays to avoid expensive GetPixel calls.
/// Caches by instance and does not own the texture; entries are not removed automatically.
/// </summary>
public static class LutCache
{
    static readonly Dictionary<Texture2D, Color[]> Cache = new();

    public static bool TryGetPixels(Texture2D lut, out Color[] pixels)
    {
        if (lut == null)
        {
            pixels = null;
            return false;
        }

        if (!lut.isReadable)
        {
            pixels = null;
            return false;
        }

        if (Cache.TryGetValue(lut, out pixels)) return true;

        try
        {
            pixels = lut.GetPixels();
            Cache[lut] = pixels;
            return true;
        }
        catch
        {
            pixels = null;
            return false;
        }
    }
}
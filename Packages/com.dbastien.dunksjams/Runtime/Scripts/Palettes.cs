using UnityEngine;

/// <summary>
/// Color theory utilities and pregenerated palette arrays.
/// Complements ColorExtensions palette methods (MonochromaticPalette, ComplementaryPalette, etc.).
/// </summary>
public static class Palettes
{
    /// <summary>Hue offset for complementary colors (180°).</summary>
    public const float ComplementaryHueOffset = 0.5f;

    /// <summary>Hue offset for triadic colors (120°).</summary>
    public const float TriadicHueOffset = 1f / 3f;

    /// <summary>Hue offset for split-complementary (±30° from complement).</summary>
    public const float SplitComplementaryOffset = 30f / 360f;

    /// <summary>Hue offset for tetradic rectangle (90°).</summary>
    public const float TetradicOffset = 0.25f;

    /// <summary>Hue offset for square scheme (90°).</summary>
    public const float SquareOffset = 0.25f;

    // Color theory: generate palettes from base color

    public static Color[] SplitComplementary(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        return new[]
        {
            Color.HSVToRGB(h, s, v),
            Color.HSVToRGB(Mathf.Repeat(h + 0.5f - SplitComplementaryOffset, 1f), s, v),
            Color.HSVToRGB(Mathf.Repeat(h + 0.5f + SplitComplementaryOffset, 1f), s, v)
        };
    }

    public static Color[] Tetradic(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        return new[]
        {
            Color.HSVToRGB(h, s, v),
            Color.HSVToRGB(Mathf.Repeat(h + TetradicOffset, 1f), s, v),
            Color.HSVToRGB(Mathf.Repeat(h + ComplementaryHueOffset, 1f), s, v),
            Color.HSVToRGB(Mathf.Repeat(h + ComplementaryHueOffset + TetradicOffset, 1f), s, v)
        };
    }

    public static Color[] SquareScheme(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        return new[]
        {
            Color.HSVToRGB(h, s, v),
            Color.HSVToRGB(Mathf.Repeat(h + SquareOffset, 1f), s, v),
            Color.HSVToRGB(Mathf.Repeat(h + SquareOffset * 2f, 1f), s, v),
            Color.HSVToRGB(Mathf.Repeat(h + SquareOffset * 3f, 1f), s, v)
        };
    }

    public static Color[] TetradicSplit(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        var half = TetradicOffset * 0.5f;
        return new[]
        {
            Color.HSVToRGB(h, s, v),
            Color.HSVToRGB(Mathf.Repeat(h + half, 1f), s, v),
            Color.HSVToRGB(Mathf.Repeat(h + ComplementaryHueOffset, 1f), s, v),
            Color.HSVToRGB(Mathf.Repeat(h + ComplementaryHueOffset + half, 1f), s, v)
        };
    }

    /// <summary>Generate N evenly spaced hues on the wheel, optionally with varied saturation/value.</summary>
    public static Color[] EvenHuePalette(int count, float saturation = 0.8f, float value = 0.9f)
    {
        var palette = new Color[count];
        for (var i = 0; i < count; i++)
            palette[i] = Color.HSVToRGB((float)i / count, saturation, value);
        return palette;
    }

    /// <summary>Complementary palette with optional tints/shades (varying saturation/value).</summary>
    public static Color[] ComplementaryWithVariants(Color baseColor, int variantsPerHue = 2)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        var palette = new Color[variantsPerHue * 2];
        for (var i = 0; i < variantsPerHue; i++)
        {
            var vS = Mathf.Lerp(0.4f, 1f, (float)(i + 1) / (variantsPerHue + 1));
            var sS = Mathf.Lerp(0.5f, s, (float)(i + 1) / (variantsPerHue + 1));
            palette[i] = Color.HSVToRGB(h, sS, vS);
            palette[variantsPerHue + i] = Color.HSVToRGB(Mathf.Repeat(h + ComplementaryHueOffset, 1f), sS, vS);
        }

        return palette;
    }

    /// <summary>Single complementary color (180° on hue wheel).</summary>
    public static Color Complement(Color color)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        return Color.HSVToRGB(Mathf.Repeat(h + ComplementaryHueOffset, 1f), s, v).WithAlpha(color.a);
    }

    /// <summary>Grayscale ramp plus accent. Useful for UI (neutral grays with one brand color).</summary>
    public static Color[] AchromaticAccent(Color accent, int grayCount = 5)
    {
        grayCount = Mathf.Max(2, grayCount);
        var palette = new Color[grayCount + 1];
        for (var i = 0; i < grayCount; i++)
        {
            var g = Mathf.Lerp(0.15f, 0.9f, (float)i / (grayCount - 1));
            palette[i] = new Color(g, g, g);
        }

        palette[grayCount] = accent;
        return palette;
    }

    // ---------------------------------------------------------------------------
    // LUT color grading (requires Texture2D Read/Write enabled)
    // ---------------------------------------------------------------------------

    /// <summary>Transform a color through a 2D LUT (16³ or 32³ format). Returns input unchanged if LUT is null or not readable.</summary>
    public static Color ApplyLut(Color color, Texture2D lut)
    {
        if (!lut || !lut.isReadable) return color;
        var h = lut.height;
        if (h != 16 && h != 32) return color;
        var size = h;
        var r = Mathf.Clamp01(color.r);
        var g = Mathf.Clamp01(color.g);
        var b = Mathf.Clamp01(color.b);
        var ri = Mathf.Clamp(Mathf.FloorToInt(r * size), 0, size - 1);
        var gi = Mathf.Clamp(Mathf.FloorToInt(g * size), 0, size - 1);
        var bi = Mathf.Clamp(Mathf.FloorToInt(b * size), 0, size - 1);
        var x = ri + gi * size;
        var y = bi;
        var outColor = lut.GetPixel(x, y);
        return new Color(outColor.r, outColor.g, outColor.b, color.a);
    }

    /// <summary>Transform multiple colors through a LUT.</summary>
    public static Color[] FromLut(Color[] inputColors, Texture2D lut)
    {
        if (inputColors == null || inputColors.Length == 0) return inputColors;
        var result = new Color[inputColors.Length];
        for (var i = 0; i < inputColors.Length; i++)
            result[i] = ApplyLut(inputColors[i], lut);
        return result;
    }

    /// <summary>All discovered LUTs from LutRegistry. Run DunksJams‽/Refresh LUT Registry to populate.</summary>
    public static LutRegistry DiscoveredLuts =>
        _discoveredLuts ??= Resources.Load<LutRegistry>("LutRegistry");

    static LutRegistry _discoveredLuts;

    // Pregenerated palettes

    public static readonly Color[] WarmSunset = new[]
    {
        new Color(1f, 0.45f, 0.35f),
        new Color(1f, 0.6f, 0.4f),
        new Color(1f, 0.78f, 0.5f),
        new Color(0.95f, 0.55f, 0.4f),
        new Color(0.85f, 0.4f, 0.35f)
    };

    public static readonly Color[] Ocean = new[]
    {
        new Color(0.1f, 0.2f, 0.4f),
        new Color(0.15f, 0.35f, 0.55f),
        new Color(0.25f, 0.5f, 0.7f),
        new Color(0.4f, 0.65f, 0.85f),
        new Color(0.6f, 0.8f, 0.95f)
    };

    public static readonly Color[] Forest = new[]
    {
        new Color(0.15f, 0.35f, 0.2f),
        new Color(0.2f, 0.45f, 0.25f),
        new Color(0.35f, 0.55f, 0.3f),
        new Color(0.5f, 0.65f, 0.4f),
        new Color(0.65f, 0.75f, 0.55f)
    };

    public static readonly Color[] Neon = new[]
    {
        new Color(1f, 0.2f, 0.6f),
        new Color(0.2f, 1f, 0.6f),
        new Color(0.2f, 0.6f, 1f),
        new Color(1f, 0.9f, 0.2f),
        new Color(0.8f, 0.2f, 1f)
    };

    public static readonly Color[] Pastel = new[]
    {
        new Color(1f, 0.85f, 0.9f),
        new Color(0.85f, 0.95f, 1f),
        new Color(0.9f, 1f, 0.85f),
        new Color(1f, 0.95f, 0.85f),
        new Color(0.9f, 0.85f, 1f)
    };

    public static readonly Color[] Earth = new[]
    {
        new Color(0.4f, 0.28f, 0.2f),
        new Color(0.55f, 0.4f, 0.28f),
        new Color(0.7f, 0.55f, 0.4f),
        new Color(0.85f, 0.7f, 0.55f),
        new Color(0.65f, 0.5f, 0.35f)
    };

    public static readonly Color[] Cyberpunk = new[]
    {
        new Color(0.08f, 0.05f, 0.15f),
        new Color(0.25f, 0.1f, 0.4f),
        new Color(0f, 0.9f, 1f),
        new Color(1f, 0.1f, 0.6f),
        new Color(0.9f, 0.2f, 1f),
        new Color(0.6f, 0.95f, 1f),
        new Color(1f, 0.5f, 0.85f)
    };

    public static readonly Color[] Retro = new[]
    {
        new Color(0.2f, 0.15f, 0.12f),
        new Color(0.9f, 0.45f, 0.2f),
        new Color(0.98f, 0.75f, 0.35f),
        new Color(0.5f, 0.6f, 0.35f),
        new Color(0.4f, 0.35f, 0.5f),
        new Color(0.95f, 0.9f, 0.8f),
        new Color(0.7f, 0.3f, 0.2f)
    };

    public static readonly Color[] Arctic = new[]
    {
        new Color(0.92f, 0.95f, 1f),
        new Color(0.85f, 0.92f, 0.98f),
        new Color(0.7f, 0.88f, 0.95f),
        new Color(0.5f, 0.78f, 0.9f),
        new Color(0.35f, 0.6f, 0.78f),
        new Color(0.2f, 0.45f, 0.6f)
    };

    public static readonly Color[] Grayscale = new[]
    {
        new Color(0.1f, 0.1f, 0.1f),
        new Color(0.3f, 0.3f, 0.3f),
        new Color(0.5f, 0.5f, 0.5f),
        new Color(0.7f, 0.7f, 0.7f),
        new Color(0.9f, 0.9f, 0.9f)
    };

    public static readonly Color[] Monokai = new[]
    {
        new Color(0.98f, 0.97f, 0.84f),
        new Color(0.76f, 0.18f, 0.29f),
        new Color(0.47f, 0.92f, 0.47f),
        new Color(0.95f, 0.66f, 0.29f),
        new Color(0.4f, 0.62f, 0.89f),
        new Color(0.7f, 0.54f, 0.95f),
        new Color(0.4f, 0.81f, 0.87f)
    };

    public static readonly Color[] Dracula = new[]
    {
        new Color(0.25f, 0.25f, 0.35f),
        new Color(0.98f, 0.97f, 0.94f),
        new Color(0.98f, 0.29f, 0.35f),
        new Color(0.49f, 0.98f, 0.63f),
        new Color(0.98f, 0.82f, 0.36f),
        new Color(0.4f, 0.57f, 0.98f),
        new Color(0.8f, 0.43f, 0.98f),
        new Color(0.42f, 0.93f, 1f)
    };

    public static readonly Color[] Nord = new[]
    {
        new Color(0.18f, 0.2f, 0.25f),
        new Color(0.24f, 0.27f, 0.32f),
        new Color(0.3f, 0.34f, 0.4f),
        new Color(0.76f, 0.81f, 0.88f),
        new Color(0.92f, 0.93f, 0.95f),
        new Color(0.92f, 0.33f, 0.39f),
        new Color(0.81f, 0.47f, 0.47f),
        new Color(0.93f, 0.77f, 0.44f),
        new Color(0.73f, 0.87f, 0.49f),
        new Color(0.65f, 0.81f, 0.73f),
        new Color(0.58f, 0.73f, 0.87f),
        new Color(0.82f, 0.65f, 0.8f)
    };

    // Console / retro palettes (baked, not from LUTs)
    public static readonly Color[] GameBoy = new[]
    {
        new Color(0.06f, 0.22f, 0.06f),
        new Color(0.19f, 0.38f, 0.19f),
        new Color(0.55f, 0.67f, 0.06f),
        new Color(0.61f, 0.74f, 0.06f)
    };

    public static readonly Color[] NES = new[]
    {
        new Color(0.43f, 0.43f, 0.43f),
        new Color(0f, 0.14f, 0.57f),
        new Color(0f, 0f, 0.86f),
        new Color(0.43f, 0.29f, 0.86f),
        new Color(0.57f, 0f, 0.43f),
        new Color(0.71f, 0f, 0.43f),
        new Color(0.71f, 0.14f, 0f),
        new Color(0.57f, 0.29f, 0f),
        new Color(0.43f, 0.29f, 0f),
        new Color(0.14f, 0.29f, 0f),
        new Color(0f, 0.43f, 0.14f),
        new Color(0f, 0.57f, 0f),
        new Color(0f, 0.29f, 0.29f),
        new Color(0f, 0f, 0f),
        new Color(0.71f, 0.71f, 0.71f),
        new Color(0f, 0.43f, 0.86f),
        new Color(0f, 0.29f, 1f),
        new Color(0.57f, 0f, 1f),
        new Color(0.71f, 0f, 0.57f),
        new Color(1f, 0f, 0f)
    };

    public static readonly Color[] C64 = new[]
    {
        new Color(0f, 0f, 0f),
        new Color(1f, 1f, 1f),
        new Color(0.53f, 0f, 0f),
        new Color(0.67f, 1f, 0.93f),
        new Color(0.8f, 0.27f, 0.8f),
        new Color(0f, 0.8f, 0.33f),
        new Color(0f, 0f, 0.67f),
        new Color(0.93f, 0.93f, 0.47f),
        new Color(0.87f, 0.53f, 0.33f),
        new Color(0.4f, 0.27f, 0f),
        new Color(1f, 0.47f, 0.47f),
        new Color(0.2f, 0.2f, 0.2f),
        new Color(0.47f, 0.47f, 0.47f),
        new Color(0.67f, 1f, 0.4f),
        new Color(0f, 0.53f, 1f),
        new Color(0.73f, 0.73f, 0.73f)
    };

    public static readonly Color[] Arne16 = new[]
    {
        new Color(0f, 0f, 0f),
        new Color(0.29f, 0.24f, 0.17f),
        new Color(0.75f, 0.15f, 0.2f),
        new Color(0.88f, 0.44f, 0.55f),
        new Color(0.62f, 0.62f, 0.62f),
        new Color(0.64f, 0.39f, 0.13f),
        new Color(0.92f, 0.54f, 0.19f),
        new Color(0.97f, 0.89f, 0.42f),
        new Color(1f, 1f, 1f),
        new Color(0.11f, 0.15f, 0.2f),
        new Color(0.18f, 0.28f, 0.31f),
        new Color(0.27f, 0.54f, 0.1f),
        new Color(0.64f, 0.81f, 0.15f),
        new Color(0f, 0.34f, 0.52f),
        new Color(0.19f, 0.64f, 0.95f),
        new Color(0.7f, 0.86f, 0.94f)
    };
}
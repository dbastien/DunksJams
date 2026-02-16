using UnityEngine;

/// <summary>
/// Color theory helpers: conversions and palette generators. Includes RGB->Lab conversions for perceptual distance.
/// </summary>
public static class ColorTheory
{
    // -- Palette generators (simple HSV-based helpers)
    public static Color Complementary(Color c)
    {
        Color.RGBToHSV(c, out var h, out var s, out var v);
        return Color.HSVToRGB(Mathf.Repeat(h + 0.5f, 1f), s, v).WithAlpha(c.a);
    }

    public static Color[] Analogous(Color c, int count = 5, float stepDegrees = 30f)
    {
        Color.RGBToHSV(c, out var h, out var s, out var v);
        var palette = new Color[count];
        var step = stepDegrees / 360f;
        for (var i = 0; i < count; i++)
            palette[i] = Color.HSVToRGB(Mathf.Repeat(h + (i - count / 2) * step, 1f), s, v).WithAlpha(c.a);
        return palette;
    }

    public static Color[] Triadic(Color c)
    {
        Color.RGBToHSV(c, out var h, out var s, out var v);
        return new[]
        {
            Color.HSVToRGB(h, s, v).WithAlpha(c.a),
            Color.HSVToRGB(Mathf.Repeat(h + 1f / 3f, 1f), s, v).WithAlpha(c.a),
            Color.HSVToRGB(Mathf.Repeat(h + 2f / 3f, 1f), s, v).WithAlpha(c.a)
        };
    }

    public static Color[] TintsAndShades(Color c, int steps = 5)
    {
        var colors = new Color[steps];
        for (var i = 0; i < steps; ++i)
        {
            var t = (float)i / (steps - 1);
            // lerp between white and c for tints, and between c and black for shades
            colors[i] = Color.Lerp(Color.white, c, t);
        }

        return colors;
    }

    public static Color[] EvenHuePalette(int count, float saturation = 0.8f, float value = 0.9f)
    {
        var palette = new Color[count];
        for (var i = 0; i < count; ++i)
            palette[i] = Color.HSVToRGB((float)i / count, saturation, value);
        return palette;
    }

    // -- Perceptual conversion helpers (sRGB -> CIE Lab)
    // Reference white (D65)
    const float Xn = 95.047f;
    const float Yn = 100f;
    const float Zn = 108.883f;

    static float PivotRgb(float n) => (n > 0.04045f) ? Mathf.Pow((n + 0.055f) / 1.055f, 2.4f) : n / 12.92f;
    static float PivotXyz(float n) => (n > 0.008856f) ? Mathf.Pow(n, 1f / 3f) : (7.787f * n) + (16f / 116f);

    public static void ColorToLab(Color c, out float L, out float a, out float b)
    {
        // Convert sRGB [0..1] to XYZ
        var r = PivotRgb(c.r) * 100f;
        var g = PivotRgb(c.g) * 100f;
        var bl = PivotRgb(c.b) * 100f;

        // Observer = 2°, Illuminant = D65
        var x = r * 0.4124f + g * 0.3576f + bl * 0.1805f;
        var y = r * 0.2126f + g * 0.7152f + bl * 0.0722f;
        var z = r * 0.0193f + g * 0.1192f + bl * 0.9505f;

        var fx = PivotXyz(x / Xn);
        var fy = PivotXyz(y / Yn);
        var fz = PivotXyz(z / Zn);

        L = (116f * fy) - 16f;
        a = 500f * (fx - fy);
        b = 200f * (fy - fz);
    }

    public static float DeltaE(Color c1, Color c2)
    {
        ColorToLab(c1, out var L1, out var a1, out var b1);
        ColorToLab(c2, out var L2, out var a2, out var b2);
        var dL = L1 - L2;
        var da = a1 - a2;
        var db = b1 - b2;
        return Mathf.Sqrt(dL * dL + da * da + db * db);
    }
}
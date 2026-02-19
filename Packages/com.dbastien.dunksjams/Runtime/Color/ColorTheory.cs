using System;
using UnityEngine;

public static class ColorTheory
{
    private static Color WithAlpha(Color c, float a) { c.a = a; return c; }

    public static Color Complementary(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        return WithAlpha(Color.HSVToRGB(Mathf.Repeat(h + 0.5f, 1f), s, v), c.a);
    }

    public static Color[] Analogous(Color c, int count = 5, float stepDegrees = 30f)
    {
        if (count <= 0) return Array.Empty<Color>();

        Color.RGBToHSV(c, out float h, out float s, out float v);
        var palette = new Color[count];

        float step = stepDegrees / 360f;
        float half = (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
            palette[i] = WithAlpha(Color.HSVToRGB(Mathf.Repeat(h + (i - half) * step, 1f), s, v), c.a);

        return palette;
    }

    public static Color[] Triadic(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        return new[]
        {
            WithAlpha(Color.HSVToRGB(h, s, v), c.a),
            WithAlpha(Color.HSVToRGB(Mathf.Repeat(h + 1f / 3f, 1f), s, v), c.a),
            WithAlpha(Color.HSVToRGB(Mathf.Repeat(h + 2f / 3f, 1f), s, v), c.a)
        };
    }

    public static Color[] TintsAndShades(Color c, int steps = 5)
    {
        steps = Mathf.Max(1, steps);
        if (steps == 1) return new[] { c };

        var colors = new Color[steps];
        int mid = (steps - 1) / 2;

        for (int i = 0; i < steps; i++)
        {
            Color outC;

            if (i <= mid)
            {
                float t = mid == 0 ? 1f : (float)i / mid;
                outC = Color.Lerp(Color.white, c, t);
            }
            else
            {
                float t = (float)(i - mid) / (steps - 1 - mid);
                outC = Color.Lerp(c, Color.black, t);
            }

            outC.a = c.a;
            colors[i] = outC;
        }

        return colors;
    }

    public static Color[] EvenHuePalette(int count, float saturation = 0.8f, float value = 0.9f)
    {
        if (count <= 0) return Array.Empty<Color>();

        var palette = new Color[count];
        for (int i = 0; i < count; i++)
            palette[i] = Color.HSVToRGB((float)i / count, saturation, value);

        return palette;
    }

    private const float Xn = 95.047f;
    private const float Yn = 100f;
    private const float Zn = 108.883f;

    private static float PivotRgb(float n) => n > 0.04045f ? Mathf.Pow((n + 0.055f) / 1.055f, 2.4f) : n / 12.92f;
    private static float PivotXyz(float n) => n > 0.008856f ? Mathf.Pow(n, 1f / 3f) : 7.787f * n + 16f / 116f;

    public static void ColorToLab(Color c, out float L, out float a, out float b)
    {
        float r = PivotRgb(c.r) * 100f;
        float g = PivotRgb(c.g) * 100f;
        float bl = PivotRgb(c.b) * 100f;

        float x = r * 0.4124f + g * 0.3576f + bl * 0.1805f;
        float y = r * 0.2126f + g * 0.7152f + bl * 0.0722f;
        float z = r * 0.0193f + g * 0.1192f + bl * 0.9505f;

        float fx = PivotXyz(x / Xn);
        float fy = PivotXyz(y / Yn);
        float fz = PivotXyz(z / Zn);

        L = 116f * fy - 16f;
        a = 500f * (fx - fy);
        b = 200f * (fy - fz);
    }

    public static float DeltaE(Color c1, Color c2)
    {
        ColorToLab(c1, out float L1, out float a1, out float b1);
        ColorToLab(c2, out float L2, out float a2, out float b2);

        float dL = L1 - L2;
        float da = a1 - a2;
        float db = b1 - b2;

        return Mathf.Sqrt(dL * dL + da * da + db * db);
    }
}
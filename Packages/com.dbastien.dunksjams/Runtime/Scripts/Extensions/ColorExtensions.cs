using UnityEngine;

public static class ColorExtensions
{
    public static Color Sub(this Color c, float val) => new(c.r - val, c.g - val, c.b - val, c.a);
    public static Color Add(this Color c, float val) => new(c.r + val, c.g + val, c.b + val, c.a);

    public static Color LerpUnclamped(this Color l, Color r, float t) => l + t * (r - l);
    public static Color PremultiplyAlpha(this Color c) => new(c.r * c.a, c.g * c.a, c.b * c.a, c.a);

    public static float PerceivedBrightness(this Color c) => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
    public static bool IsDarkerThan(this Color c, float threshold = 0.1f) => c.PerceivedBrightness() < threshold;
    public static bool IsLighterThan(this Color c, float threshold = 0.9f) => c.PerceivedBrightness() > threshold;

    public static Color Min(this Color a, Color b) => new(
        Mathf.Min(a.r, b.r),
        Mathf.Min(a.g, b.g),
        Mathf.Min(a.b, b.b),
        Mathf.Min(a.a, b.a)
    );

    public static Color Max(this Color a, Color b) => new(
        Mathf.Max(a.r, b.r),
        Mathf.Max(a.g, b.g),
        Mathf.Max(a.b, b.b),
        Mathf.Max(a.a, b.a)
    );

    public static Color Max(this Color c, float v) => new(
        Mathf.Max(c.r, v),
        Mathf.Max(c.g, v),
        Mathf.Max(c.b, v),
        Mathf.Max(c.a, v)
    );

    public static Color Div(Color n, Color d) => new(
        n.r / d.r,
        n.g / d.g,
        n.b / d.b,
        n.a / d.a
    );

    public static Color Saturate(Color c) => new(
        Mathf.Clamp01(c.r),
        Mathf.Clamp01(c.g),
        Mathf.Clamp01(c.b),
        Mathf.Clamp01(c.a)
    );

    public static float MaxRgb(this Color c) => Mathf.Max(c.r, Mathf.Max(c.g, c.b));

    // ---------------------------
    // Blend modes (RGB-ish; alpha comes along for the ride via Color ops)
    // https://www.wikiwand.com/en/Blend_modes
    // ---------------------------

    public static Color BlendSoftLight(this Color a, Color b) =>
        // (1 - 2b) * a^2 + 2b * a
        // Need Color.white instead of "1f - color".
        (Color.white - 2f * b) * (a * a) + 2f * b * a;

    // darken modes
    public static Color BlendMultiply(this Color a, Color b) => a * b;

    public static Color BlendColorBurn(this Color a, Color b)
    {
        // 1 - (1 - b) / a
        // Clamp + avoid div-by-zero per channel.
        var denom = a.Max(float.Epsilon);
        return Saturate(Color.white - Div(Color.white - b, denom));
    }

    public static Color BlendLinearBurn(this Color a, Color b) => (a + b).Sub(1f);
    public static Color BlendDarkenOnly(this Color a, Color b) => a.Min(b);

    // lighten modes
    public static Color BlendScreen(this Color a, Color b) => Color.white - (Color.white - a) * (Color.white - b);

    public static Color BlendColorDodge(this Color a, Color b)
    {
        // b / (1 - a)
        var denom = (Color.white - a).Max(float.Epsilon);
        return Saturate(Div(b, denom));
    }

    public static Color BlendLinearDodge(this Color a, Color b) => a + b;
    public static Color BlendLightenOnly(this Color a, Color b) => a.Max(b);
    public static Color BlendSubtract(this Color a, Color b) => b - a;

    public static Color BlendDivide(this Color a, Color b)
    {
        // b / a
        var denom = a.Max(float.Epsilon);
        return Saturate(Div(b, denom));
    }

    public static Color[] MonochromaticPalette(this Color col, int count = 5)
    {
        Color.RGBToHSV(col, out var h, out _, out _);
        var palette = new Color[count];

        for (var i = 0; i < count; ++i)
        {
            var s = Mathf.Lerp(0.3f, 1f, (float)i / (count - 1));
            var v = Mathf.Lerp(0.9f, 0.3f, (float)i / (count - 1));
            palette[i] = Color.HSVToRGB(h, s, v);
        }

        return palette;
    }

    public static Color[] ComplementaryPalette(this Color col)
    {
        Color.RGBToHSV(col, out var h, out var s, out var v);
        return new[]
        {
            Color.HSVToRGB(h, s, v),
            Color.HSVToRGB(Mathf.Repeat(h + 0.5f, 1f), s, v)
        };
    }

    public static Color[] AnalogousPalette(this Color col, int count = 5)
    {
        Color.RGBToHSV(col, out var h, out var s, out var v);
        var palette = new Color[count];

        var hueStep = 30f / 360f;

        for (var i = 0; i < count; i++)
        {
            var hueOffset = (i - count / 2) * hueStep;
            palette[i] = Color.HSVToRGB(Mathf.Repeat(h + hueOffset, 1f), s, v);
        }

        return palette;
    }

    public static Color[] TriadicPalette(this Color col)
    {
        Color.RGBToHSV(col, out var h, out var s, out var v);
        return new[]
        {
            Color.HSVToRGB(h, s, v),
            Color.HSVToRGB(Mathf.Repeat(h + 1f / 3f, 1f), s, v),
            Color.HSVToRGB(Mathf.Repeat(h + 2f / 3f, 1f), s, v)
        };
    }

    public static Color HueShift(this Color color, float hueShiftDegrees)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        h = Mathf.Repeat(h + hueShiftDegrees / 360f, 1f);
        return Color.HSVToRGB(h, s, v).WithAlpha(color.a);
    }

    public static Color SaturationAdjust(this Color color, float saturationMultiplier)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        s = Mathf.Clamp01(s * saturationMultiplier);
        return Color.HSVToRGB(h, s, v).WithAlpha(color.a);
    }

    public static Color BrightnessAdjust(this Color color, float brightnessMultiplier)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        v = Mathf.Clamp01(v * brightnessMultiplier);
        return Color.HSVToRGB(h, s, v).WithAlpha(color.a);
    }

    public static Color BlendColors(params Color[] colors)
    {
        if (colors.Length == 0) return Color.black;
        if (colors.Length == 1) return colors[0];

        var result = colors[0];
        for (var i = 1; i < colors.Length; i++) result = Color.Lerp(result, colors[i], 1f / (i + 1));
        return result;
    }

    public static Color WithAlpha(this Color color, float alpha) =>
        new(color.r, color.g, color.b, alpha);

    public static bool IsApproximatelyDark(this Color color, float threshold = 0.1f) =>
        color.PerceivedBrightness() < threshold;

    public static bool IsApproximatelyLight(this Color color, float threshold = 0.9f) =>
        color.PerceivedBrightness() > threshold;

    /// <summary>Returns black or white for best contrast on the given background.</summary>
    public static Color SuggestedTextColor(this Color background, float threshold = 0.5f) =>
        background.PerceivedBrightness() >= threshold ? Color.black : Color.white;
}
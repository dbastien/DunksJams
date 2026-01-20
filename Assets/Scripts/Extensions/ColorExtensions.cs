using UnityEngine;

public static class ColorExtensions
{
    public static Color Sub(this Color c, float value) => new Color(c.r - value, c.g - value, c.b - value, c.a);
    public static Color Add(this Color c, float value) => new Color(c.r + value, c.g + value, c.b + value, c.a);

    public static Color LerpUnclamped(this Color l, Color r, float t) => l + t * (r - l);
    public static Color PremultiplyAlpha(this Color c) => new Color(c.r * c.a, c.g * c.a, c.b * c.a, c.a);

    public static float PerceivedBrightness(this Color c) => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
    public static bool IsDarkerThan(this Color c, float threshold = 0.1f) => c.PerceivedBrightness() < threshold;
    public static bool IsLighterThan(this Color c, float threshold = 0.9f) => c.PerceivedBrightness() > threshold;

    public static Color Min(this Color a, Color b) => new Color(
        Mathf.Min(a.r, b.r),
        Mathf.Min(a.g, b.g),
        Mathf.Min(a.b, b.b),
        Mathf.Min(a.a, b.a)
    );

    public static Color Max(this Color a, Color b) => new Color(
        Mathf.Max(a.r, b.r),
        Mathf.Max(a.g, b.g),
        Mathf.Max(a.b, b.b),
        Mathf.Max(a.a, b.a)
    );

    public static Color Max(this Color c, float v) => new Color(
        Mathf.Max(c.r, v),
        Mathf.Max(c.g, v),
        Mathf.Max(c.b, v),
        Mathf.Max(c.a, v)
    );

    public static float MaxRgb(this Color c) => Mathf.Max(c.r, Mathf.Max(c.g, c.b));

    // ---------------------------
    // Blend modes (RGB-ish; alpha comes along for the ride via Color ops)
    // https://www.wikiwand.com/en/Blend_modes
    // ---------------------------

    public static Color BlendSoftLight(this Color a, Color b)
    {
        // (1 - 2b) * a^2 + 2b * a
        // Need Color.one instead of "1f - color".
        return (Color.white - 2f * b) * (a * a) + (2f * b) * a;
    }

    // darken modes
    public static Color BlendMultiply(this Color a, Color b) => a * b;

    public static Color BlendColorBurn(this Color a, Color b)
    {
        // 1 - (1 - b) / a
        // Clamp + avoid div-by-zero per channel.
        var denom = a.Max(float.Epsilon);
        return saturate(Color.white - Div(Color.white - b, denom));
    }

    public static Color BlendLinearBurn(this Color a, Color b) => (a + b).Sub(1f);
    public static Color BlendDarkenOnly(this Color a, Color b) => a.Min(b);

    // lighten modes
    public static Color BlendScreen(this Color a, Color b) => Color.white - ((Color.white - a) * (Color.white - b));

    public static Color BlendColorDodge(this Color a, Color b)
    {
        // b / (1 - a)
        var denom = (Color.white - a).Max(float.Epsilon);
        return saturate(Div(b, denom));
    }

    public static Color BlendLinearDodge(this Color a, Color b) => a + b;
    public static Color BlendLightenOnly(this Color a, Color b) => a.Max(b);
    public static Color BlendSubtract(this Color a, Color b) => b - a;
    public static Color BlendDivide(this Color a, Color b)
    {
        // b / a
        var denom = a.Max(float.Epsilon);
        return saturate(Div(b, denom));
    }

    // ---------------------------
    // Internal math helpers
    // ---------------------------

    private static Color saturate(Color c) => new Color(
        Mathf.Clamp01(c.r),
        Mathf.Clamp01(c.g),
        Mathf.Clamp01(c.b),
        Mathf.Clamp01(c.a)
    );

    private static Color Div(Color n, Color d) => new Color(
        n.r / d.r,
        n.g / d.g,
        n.b / d.b,
        n.a / d.a
    );
}

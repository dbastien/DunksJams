using UnityEngine;

public static class Color32Extensions
{
    public static Color32 PremultiplyAlpha(this Color32 c)
    {
        float alphaFactor = c.a / 255f;
        byte r = (byte)(c.r * alphaFactor);
        byte g = (byte)(c.g * alphaFactor);
        byte b = (byte)(c.b * alphaFactor);
        return new(r, g, b, c.a);
    }

    public static Color32 AdjustAlpha(this Color32 c, float alphaMultiplier)
    {
        byte newAlpha = (byte)Mathf.Clamp(c.a * alphaMultiplier, 0, 255);
        return new(c.r, c.g, c.b, newAlpha);
    }

    // Luminance formula (ITU-R BT.709)
    public static float Brightness(this Color32 c) => (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b) / 255f;
    public static float Hue(this Color32 c)
    {
        Color.RGBToHSV(c, out float h, out _, out _);
        return h;
    }
    public static float Saturation(this Color32 c)
    {
        float r = c.r / 255f;
        float g = c.g / 255f;
        float b = c.b / 255f;

        float max = Mathf.Max(r, g, b);
        float min = Mathf.Min(r, g, b);

        if (max == 0) return 0f;

        return (max - min) / max;
    }

    public static Color32 AdjustBrightness(this Color32 c, float brightnessFactor)
    {
        // Adjust range as needed
        brightnessFactor = Mathf.Clamp(brightnessFactor, 0f, 10f);
        byte r = (byte)Mathf.Clamp(c.r * brightnessFactor, 0, 255);
        byte g = (byte)Mathf.Clamp(c.g * brightnessFactor, 0, 255);
        byte b = (byte)Mathf.Clamp(c.b * brightnessFactor, 0, 255);
        return new(r, g, b, c.a);
    }

    public static Color32 AdjustHue(this Color32 c, float hueShift)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        h = (h + hueShift) % 1f;
        if (h < 0) h += 1f;
        return Color.HSVToRGB(h, s, v);
    }

    public static Color32 AdjustSaturation(this Color32 c, float saturationMultiplier)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * saturationMultiplier);
        return Color.HSVToRGB(h, s, v);;
    }

    public static Color32 Invert(this Color32 c) => new((byte)(255 - c.r), (byte)(255 - c.g), (byte)(255 - c.b), c.a);

    public static Color32 Lerp(this Color32 a, Color32 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new(
            (byte)(a.r + (b.r - a.r) * t),
            (byte)(a.g + (b.g - a.g) * t),
            (byte)(a.b + (b.b - a.b) * t),
            (byte)(a.a + (b.a - a.a) * t)
        );
    }

    public static bool IsApproximatelyEqual(this Color32 a, Color32 b, byte tolerance = 10) =>
        Mathf.Abs(a.r - b.r) <= tolerance &&
        Mathf.Abs(a.g - b.g) <= tolerance &&
        Mathf.Abs(a.b - b.b) <= tolerance &&
        Mathf.Abs(a.a - b.a) <= tolerance;

    public static string ToHexString(this Color32 c, bool includeAlpha = true) =>
        includeAlpha
            ? $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}"
            : $"#{c.r:X2}{c.g:X2}{c.b:X2}";
}
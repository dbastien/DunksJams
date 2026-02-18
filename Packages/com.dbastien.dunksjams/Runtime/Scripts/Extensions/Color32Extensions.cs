using UnityEngine;

public static class Color32Extensions
{
    public static Color32 PremultiplyAlpha(this Color32 c)
    {
        float alphaFactor = c.a / 255f;
        return new Color32(
            (byte)(c.r * alphaFactor),
            (byte)(c.g * alphaFactor),
            (byte)(c.b * alphaFactor),
            c.a);
    }

    public static Color32 AdjustAlpha(this Color32 c, float alphaMultiplier) =>
        new(c.r, c.g, c.b, (byte)Mathf.Clamp(c.a * alphaMultiplier, 0, 255));

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

    public static Color32 AdjustBrightness(this Color32 c, float brightness, float max = 2f)
    {
        brightness = Mathf.Clamp(brightness, 0f, max);
        return new Color32(
            (byte)Mathf.Clamp(c.r * brightness, 0, 255),
            (byte)Mathf.Clamp(c.g * brightness, 0, 255),
            (byte)Mathf.Clamp(c.b * brightness, 0, 255),
            c.a
        );
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
        return Color.HSVToRGB(h, s, v);
        ;
    }

    public static Color32 AdjustContrast(this Color32 c, float contrast)
    {
        contrast = Mathf.Clamp(contrast, -1f, 1f);
        float f = 1f + contrast, mid = 128f;

        return new Color32(
            (byte)Mathf.Clamp(mid + f * (c.r - mid), 0, 255),
            (byte)Mathf.Clamp(mid + f * (c.g - mid), 0, 255),
            (byte)Mathf.Clamp(mid + f * (c.b - mid), 0, 255),
            c.a
        );
    }

    public static Color32 Grayscale(this Color32 c)
    {
        var gray = (byte)(0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b);
        return new Color32(gray, gray, gray, c.a);
    }

    public static Color32 Invert(this Color32 c) => new((byte)(255 - c.r), (byte)(255 - c.g), (byte)(255 - c.b), c.a);

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
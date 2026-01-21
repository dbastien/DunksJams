using System;
using UnityEngine;

public static class ProceduralColor
{
    public static Color RandomRGB() => new(Rand.Float(), Rand.Float(), Rand.Float());

    public static Color RandomRGB(float alpha) => new(Rand.Float(), Rand.Float(), Rand.Float(), alpha);

    public static Color RandomHSV(float? saturation = null, float? value = null, float alpha = 1f)
    {
        float h = Rand.Rad();
        float s = saturation ?? Rand.Float();
        float v = value ?? Rand.Float();
        return Color.HSVToRGB(h / MathConsts.Tau, s, v).WithAlpha(alpha);
    }

    public static Color RandomHSVInRanges(Vector2 saturationRange, Vector2 valueRange, float alpha = 1f)
    {
        float h = Rand.Rad();
        float s = Mathf.Lerp(saturationRange.x, saturationRange.y, Rand.Float());
        float v = Mathf.Lerp(valueRange.x, valueRange.y, Rand.Float());
        return Color.HSVToRGB(h / MathConsts.Tau, s, v).WithAlpha(alpha);
    }

    public static Color RandomGrayscale(float alpha = 1f)
    {
        float gray = Rand.Float();
        return new(gray, gray, gray, alpha);
    }

    public static Color RandomVariationOf(Color baseColor, float hueVariation = 30f, float satVariation = 0.2f, float valVariation = 0.2f)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);

        h += Rand.FloatRanged(-hueVariation, hueVariation) / 360f;
        h = Mathf.Repeat(h, 1f);

        s = Mathf.Clamp01(s + Rand.FloatRanged(-satVariation, satVariation));
        v = Mathf.Clamp01(v + Rand.FloatRanged(-valVariation, valVariation));

        return Color.HSVToRGB(h, s, v).WithAlpha(baseColor.a);
    }

    public static Color RandomComplementaryVariation(Color baseColor, float variation = 0.1f)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        h = Mathf.Repeat(h + 0.5f + Rand.FloatRanged(-variation, variation), 1f);
        return Color.HSVToRGB(h, s, v).WithAlpha(baseColor.a);
    }

    public static Color RandomAnalogousVariation(Color baseColor, float maxHueShift = 30f)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        h = Mathf.Repeat(h + Rand.FloatRanged(-maxHueShift, maxHueShift) / 360f, 1f);
        return Color.HSVToRGB(h, s, v).WithAlpha(baseColor.a);
    }

    public static Color WithNoise(Color color, float noiseAmount = 0.1f)
    {
        return new(
            Mathf.Clamp01(color.r + Rand.FloatRanged(-noiseAmount, noiseAmount)),
            Mathf.Clamp01(color.g + Rand.FloatRanged(-noiseAmount, noiseAmount)),
            Mathf.Clamp01(color.b + Rand.FloatRanged(-noiseAmount, noiseAmount)),
            color.a
        );
    }

    public static Color MultiColorGradient(Color[] colors, float t)
    {
        if (colors.Length == 0) return Color.black;
        if (colors.Length == 1) return colors[0];

        t = Mathf.Clamp01(t);
        float segment = 1f / (colors.Length - 1);
        int index = Mathf.FloorToInt(t / segment);
        index = Mathf.Clamp(index, 0, colors.Length - 2);

        float localT = (t - index * segment) / segment;
        return Color.Lerp(colors[index], colors[index + 1], localT);
    }

    public static Color Rainbow(float t, float saturation = 1f, float value = 1f)
    {
        t = Mathf.Repeat(t, 1f);
        return Color.HSVToRGB(t, saturation, value);
    }

    public static Color SineWaveColor(float time, float frequency = 1f, float saturation = 1f, float value = 1f)
    {
        float hue = (Mathf.Sin(time * frequency * MathConsts.Tau) + 1f) * 0.5f;
        return Color.HSVToRGB(hue, saturation, value);
    }

    public static Color[] MonochromaticPalette(Color baseColor, int count = 5)
    {
        Color.RGBToHSV(baseColor, out float h, out _, out _);
        var palette = new Color[count];

        for (int i = 0; i < count; i++)
        {
            float s = Mathf.Lerp(0.3f, 1f, (float)i / (count - 1));
            float v = Mathf.Lerp(0.9f, 0.3f, (float)i / (count - 1));
            palette[i] = Color.HSVToRGB(h, s, v);
        }

        return palette;
    }

    public static Color[] ComplementaryPalette(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        return new[]
        {
            Color.HSVToRGB(h, s, v),
            Color.HSVToRGB(Mathf.Repeat(h + 0.5f, 1f), s, v)
        };
    }

    public static Color[] AnalogousPalette(Color baseColor, int count = 5)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        var palette = new Color[count];

        float hueStep = 30f / 360f;

        for (int i = 0; i < count; i++)
        {
            float hueOffset = (i - count / 2) * hueStep;
            palette[i] = Color.HSVToRGB(Mathf.Repeat(h + hueOffset, 1f), s, v);
        }

        return palette;
    }

    public static Color[] TriadicPalette(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        return new[]
        {
            Color.HSVToRGB(h, s, v),
            Color.HSVToRGB(Mathf.Repeat(h + 1f/3f, 1f), s, v),
            Color.HSVToRGB(Mathf.Repeat(h + 2f/3f, 1f), s, v)
        };
    }

    public static (float h, float s, float v) GetHSVFromRGB(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return (h, s, v);
    }

    public static Color FromHSV(float h, float s, float v, float a = 1f) =>
        Color.HSVToRGB(h, s, v).WithAlpha(a);

    public static Color HueShift(Color color, float hueShiftDegrees)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        h = Mathf.Repeat(h + hueShiftDegrees / 360f, 1f);
        return Color.HSVToRGB(h, s, v).WithAlpha(color.a);
    }

    public static Color SaturationAdjust(Color color, float saturationMultiplier)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * saturationMultiplier);
        return Color.HSVToRGB(h, s, v).WithAlpha(color.a);
    }

    public static Color BrightnessAdjust(Color color, float brightnessMultiplier)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        v = Mathf.Clamp01(v * brightnessMultiplier);
        return Color.HSVToRGB(h, s, v).WithAlpha(color.a);
    }

    public static Color RandomFromPalette(Color[] palette)
    {
        if (palette.Length == 0) return Color.black;
        return palette[Rand.IntRanged(0, palette.Length)];
    }

    public static Color BlendColors(params Color[] colors)
    {
        if (colors.Length == 0) return Color.black;
        if (colors.Length == 1) return colors[0];

        Color result = colors[0];
        for (int i = 1; i < colors.Length; i++)
        {
            result = Color.Lerp(result, colors[i], 1f / (i + 1));
        }
        return result;
    }

    public static Color WithAlpha(this Color color, float alpha) =>
        new(color.r, color.g, color.b, alpha);

    public static float PerceivedBrightness(Color color) =>
        0.299f * color.r + 0.587f * color.g + 0.114f * color.b;

    public static bool IsApproximatelyDark(Color color, float threshold = 0.1f) =>
        PerceivedBrightness(color) < threshold;

    public static bool IsApproximatelyLight(Color color, float threshold = 0.9f) =>
        PerceivedBrightness(color) > threshold;
}
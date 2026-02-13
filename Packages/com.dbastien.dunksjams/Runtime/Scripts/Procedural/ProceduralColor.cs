using UnityEngine;

public static class ProceduralColor
{
    public static Color RandomRGB() => new(Rand.Float(), Rand.Float(), Rand.Float());

    public static Color RandomRGB(float alpha) => new(Rand.Float(), Rand.Float(), Rand.Float(), alpha);

    public static Color RandomHSV(float? saturation = null, float? value = null, float alpha = 1f)
    {
        var h = Rand.Rad();
        var s = saturation ?? Rand.Float();
        var v = value ?? Rand.Float();
        return Color.HSVToRGB(h / MathConsts.Tau, s, v).WithAlpha(alpha);
    }

    public static Color RandomHSVInRanges(Vector2 saturationRange, Vector2 valueRange, float alpha = 1f)
    {
        var h = Rand.Rad();
        var s = Mathf.Lerp(saturationRange.x, saturationRange.y, Rand.Float());
        var v = Mathf.Lerp(valueRange.x, valueRange.y, Rand.Float());
        return Color.HSVToRGB(h / MathConsts.Tau, s, v).WithAlpha(alpha);
    }

    public static Color RandomGrayscale(float alpha = 1f)
    {
        var gray = Rand.Float();
        return new Color(gray, gray, gray, alpha);
    }

    public static Color RandomVariationOf(Color baseColor, float hueVariation = 30f, float satVariation = 0.2f,
        float valVariation = 0.2f)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);

        h += Rand.FloatRanged(-hueVariation, hueVariation) / 360f;
        h = Mathf.Repeat(h, 1f);

        s = Mathf.Clamp01(s + Rand.FloatRanged(-satVariation, satVariation));
        v = Mathf.Clamp01(v + Rand.FloatRanged(-valVariation, valVariation));

        return Color.HSVToRGB(h, s, v).WithAlpha(baseColor.a);
    }

    public static Color RandomComplementaryVariation(Color baseColor, float variation = 0.1f)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        h = Mathf.Repeat(h + 0.5f + Rand.FloatRanged(-variation, variation), 1f);
        return Color.HSVToRGB(h, s, v).WithAlpha(baseColor.a);
    }

    public static Color RandomAnalogousVariation(Color baseColor, float maxHueShift = 30f)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        h = Mathf.Repeat(h + Rand.FloatRanged(-maxHueShift, maxHueShift) / 360f, 1f);
        return Color.HSVToRGB(h, s, v).WithAlpha(baseColor.a);
    }

    public static Color WithNoise(Color color, float noiseAmount = 0.1f) =>
        new(
            Mathf.Clamp01(color.r + Rand.FloatRanged(-noiseAmount, noiseAmount)),
            Mathf.Clamp01(color.g + Rand.FloatRanged(-noiseAmount, noiseAmount)),
            Mathf.Clamp01(color.b + Rand.FloatRanged(-noiseAmount, noiseAmount)),
            color.a
        );

    public static Color MultiColorGradient(Color[] colors, float t)
    {
        if (colors.Length == 0) return Color.black;
        if (colors.Length == 1) return colors[0];

        t = Mathf.Clamp01(t);
        var segment = 1f / (colors.Length - 1);
        var index = Mathf.FloorToInt(t / segment);
        index = Mathf.Clamp(index, 0, colors.Length - 2);

        var localT = (t - index * segment) / segment;
        return Color.Lerp(colors[index], colors[index + 1], localT);
    }

    public static Color Rainbow(float t, float saturation = 1f, float value = 1f)
    {
        t = Mathf.Repeat(t, 1f);
        return Color.HSVToRGB(t, saturation, value);
    }

    public static Color SineWaveColor(float time, float frequency = 1f, float saturation = 1f, float value = 1f)
    {
        var hue = (Mathf.Sin(time * frequency * MathConsts.Tau) + 1f) * 0.5f;
        return Color.HSVToRGB(hue, saturation, value);
    }

    public static Color RandomFromPalette(Color[] palette)
    {
        if (palette.Length == 0) return Color.black;
        return palette[Rand.IntRanged(0, palette.Length)];
    }
}
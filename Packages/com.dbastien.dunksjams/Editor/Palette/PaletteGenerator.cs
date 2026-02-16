#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public static class PaletteGenerator
{
    public static Color[] Generate(ColorPalette palette)
    {
        if (palette == null) return new Color[0];

        var baseColors = BuildBaseColors(palette);
        if (baseColors.Count == 0) return new Color[0];

        var shades = Mathf.Max(1, palette.Shades);
        var colors = new Color[baseColors.Count * shades];

        var baseV = Mathf.Clamp01(palette.Value);
        var minV = Mathf.Clamp01(palette.MinBrightness);
        var maxV = Mathf.Clamp01(palette.MaxBrightness);
        if (minV > maxV) (minV, maxV) = (maxV, minV);

        var darkRows = Mathf.CeilToInt((shades - 1) / 2f);
        var centerRow = darkRows;

        for (var row = 0; row < shades; row++)
        {
            var v = baseV;
            if (shades > 1 && row != centerRow)
            {
                if (row < centerRow)
                {
                    var k = centerRow - row;
                    var t = (k - 0.5f) / Mathf.Max(darkRows, 1);
                    v = Mathf.Lerp(baseV, minV, t);
                }
                else
                {
                    var lightRows = Mathf.Max(shades - 1 - darkRows, 0);
                    var k = row - centerRow;
                    var t = (k - 0.5f) / Mathf.Max(lightRows, 1);
                    v = Mathf.Lerp(baseV, maxV, t);
                }
            }

            for (var col = 0; col < baseColors.Count; col++)
            {
                var baseColor = baseColors[col];
                Color.RGBToHSV(baseColor, out var h, out var s, out _);
                var color = Color.HSVToRGB(h, s, v);
                color.a = baseColor.a;
                colors[row * baseColors.Count + col] = color;
            }
        }

        return colors;
    }

    public static int GetMinHueCount(PaletteScheme scheme)
    {
        switch (scheme)
        {
            case PaletteScheme.Custom:
                return 1;
            case PaletteScheme.Monochromatic:
                return 1;
            case PaletteScheme.Complementary:
                return 2;
            case PaletteScheme.SplitComplementary:
            case PaletteScheme.Analogous:
            case PaletteScheme.Triadic:
                return 3;
            case PaletteScheme.Tetradic:
            case PaletteScheme.Square:
                return 4;
            case PaletteScheme.Spectrum:
                return 8;
            case PaletteScheme.UI_Kit:
                return 6;
            default:
                return 1;
        }
    }

    public static List<Color> BuildBaseColors(ColorPalette palette)
    {
        var colors = new List<Color>();
        if (palette == null) return colors;

        var minHues = GetMinHueCount(palette.Scheme);
        var colorsCount = Mathf.Clamp(Mathf.Max(palette.HueCount, minHues), 1, 256);

        var alpha = palette.BaseColor.a;
        Color.RGBToHSV(palette.BaseColor, out var baseH, out _, out _);
        var baseS = Mathf.Clamp01(palette.Saturation);
        var baseV = Mathf.Clamp01(palette.Value);

        Color Primary() => Color.HSVToRGB(baseH, baseS, baseV).WithAlpha(alpha);
        Color Hue(float h) => Color.HSVToRGB(Mathf.Repeat(h, 1f), baseS, baseV).WithAlpha(alpha);

        var primary = Primary();
        var comp1 = Hue(baseH + 0.5f);
        var split = Mathf.Clamp(palette.SplitComplementaryDegrees, 1f, 180f) / 360f;
        var ana = Mathf.Clamp(palette.AnalogousStepDegrees, 1f, 180f) / 360f;
        var comp2Split = Hue(baseH + 0.5f - split);
        var comp3Split = Hue(baseH + 0.5f + split);
        var ana1 = Hue(baseH - ana);
        var ana2 = Hue(baseH + ana);
        var tri1 = Hue(baseH + 1f / 3f);
        var tri2 = Hue(baseH + 2f / 3f);
        var tet1 = Hue(baseH + 0.25f);
        var tet2 = Hue(baseH + 0.5f);
        var tet3 = Hue(baseH + 0.75f);

        switch (palette.Scheme)
        {
            case PaletteScheme.Monochromatic:
                for (var k = 0; k < colorsCount; k++) colors.Add(primary);
                break;
            case PaletteScheme.Complementary:
                for (var k = 0; k < colorsCount; k++)
                {
                    var t = colorsCount > 1 ? (float)k / (colorsCount - 1) : 0f;
                    colors.Add(Color.Lerp(primary, comp1, t));
                }
                break;
            case PaletteScheme.SplitComplementary:
                for (var k = 0; k < colorsCount; k++)
                {
                    var t = (float)k / colorsCount;
                    if (t < 1f / 3f)
                        colors.Add(Color.Lerp(primary, comp2Split, t / (1f / 3f)));
                    else if (t < 2f / 3f)
                        colors.Add(Color.Lerp(comp2Split, comp3Split, (t - 1f / 3f) / (1f / 3f)));
                    else
                        colors.Add(Color.Lerp(comp3Split, primary, (t - 2f / 3f) / (1f / 3f)));
                }
                break;
            case PaletteScheme.Analogous:
                for (var k = 0; k < colorsCount; k++)
                {
                    var t = colorsCount > 1 ? (float)k / (colorsCount - 1) : 0f;
                    if (t < 0.5f)
                        colors.Add(Color.Lerp(ana1, primary, t / 0.5f));
                    else
                        colors.Add(Color.Lerp(primary, ana2, (t - 0.5f) / 0.5f));
                }
                break;
            case PaletteScheme.Triadic:
                for (var k = 0; k < colorsCount; k++)
                {
                    var t = (float)k / colorsCount;
                    if (t < 1f / 3f)
                        colors.Add(Color.Lerp(primary, tri1, t / (1f / 3f)));
                    else if (t < 2f / 3f)
                        colors.Add(Color.Lerp(tri1, tri2, (t - 1f / 3f) / (1f / 3f)));
                    else
                        colors.Add(Color.Lerp(tri2, primary, (t - 2f / 3f) / (1f / 3f)));
                }
                break;
            case PaletteScheme.Tetradic:
            case PaletteScheme.Square:
                for (var k = 0; k < colorsCount; k++)
                {
                    var t = (float)k / colorsCount;
                    if (t < 0.25f)
                        colors.Add(Color.Lerp(primary, tet1, t / 0.25f));
                    else if (t < 0.5f)
                        colors.Add(Color.Lerp(tet1, tet2, (t - 0.25f) / 0.25f));
                    else if (t < 0.75f)
                        colors.Add(Color.Lerp(tet2, tet3, (t - 0.5f) / 0.25f));
                    else
                        colors.Add(Color.Lerp(tet3, primary, (t - 0.75f) / 0.25f));
                }
                break;
            case PaletteScheme.Spectrum:
                var includeEndpoints = palette.SpectrumIncludeBlackWhite;
                var rampCount = colorsCount;
                if (includeEndpoints)
                {
                    colors.Add(Color.black.WithAlpha(alpha));
                    colors.Add(Color.white.WithAlpha(alpha));
                    rampCount = Mathf.Max(0, colorsCount - 2);
                }

                for (var k = 0; k < rampCount; k++)
                {
                    var t = rampCount > 0 ? (float)k / rampCount : 0f;
                    var h = Mathf.Repeat(baseH + t, 1f);
                    colors.Add(Color.HSVToRGB(h, baseS, baseV).WithAlpha(alpha));
                }
                break;
            case PaletteScheme.UI_Kit:
                colors.Add(primary); // Primary
                colors.Add(Hue(baseH + 0.1f)); // Secondary
                colors.Add(Hue(baseH + 0.5f)); // Accent
                colors.Add(Color.HSVToRGB(baseH, baseS * 0.15f, 0.15f).WithAlpha(alpha)); // Background
                colors.Add(Color.HSVToRGB(baseH, baseS * 0.2f, 0.25f).WithAlpha(alpha)); // Surface
                colors.Add(Color.HSVToRGB(baseH, baseS * 0.05f, 0.95f).WithAlpha(alpha)); // Text
                break;
            case PaletteScheme.Custom:
            default:
                colors.Add(primary);
                break;
        }

        return colors;
    }

    public static int GetEffectiveHueCount(ColorPalette palette) => BuildBaseColors(palette).Count;
}
#endif
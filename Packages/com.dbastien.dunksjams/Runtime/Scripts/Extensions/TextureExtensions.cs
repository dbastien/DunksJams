using System;
using System.Collections.Generic;
using UnityEngine;

public static class Texture2DExtensions
{
    public static Texture2D ColorSwap(this Texture2D tex, Dictionary<Color, Color> swaps)
    {
        Texture2D outTex = new(tex.width, tex.height, tex.format, false);
        var pixels = tex.GetPixels();
        var outPixels = new Color[tex.width * tex.height];

        for (var i = 0; i < pixels.Length; ++i)
        {
            var originalColor = pixels[i];
            if (swaps.TryGetValue(originalColor, out var newColor))
            {
                newColor.a = originalColor.a; // Preserve alpha
                outPixels[i] = newColor;
            }
            else
            {
                outPixels[i] = originalColor;
            }
        }

        outTex.SetPixels(outPixels);
        outTex.Apply();
        return outTex;
    }

    public static Texture2D ColorShift(this Texture2D tex, Color from, Color to, float ignoreThreshold, float falloff)
    {
        Texture2D outTex = new(tex.width, tex.height, tex.format, false);
        var pixels = tex.GetPixels();
        var outPixels = new Color[tex.width * tex.height];

        for (var i = 0; i < pixels.Length; ++i)
        {
            var cOld = pixels[i];

            var rDiff = MathF.Abs(cOld.r - from.r);
            var gDiff = MathF.Abs(cOld.g - from.g);
            var bDiff = MathF.Abs(cOld.b - from.b);

            var diffFactor = (rDiff + gDiff + bDiff) / 3f;

            if (diffFactor >= ignoreThreshold)
                diffFactor = 1f;

            var lerpValue = MathF.Max(1f - diffFactor, 0f);
            var cNew = Color.Lerp(cOld, to, MathF.Pow(lerpValue, falloff));
            cNew.a = cOld.a; // Preserve alpha?

            outPixels[i] = cNew;
        }

        outTex.SetPixels(outPixels);
        outTex.Apply();
        return outTex;
    }

    public static Texture2D ScaleNX(this Texture2D tex, int factor)
    {
        var reduced = factor;
        while (reduced % 2 == 0) reduced /= 2;
        while (reduced % 3 == 0) reduced /= 3;
        if (reduced != 1)
            throw new ArgumentException("Unsupported scale factor. Factor must be a multiple of 2 or 3.",
                nameof(factor));

        var result = tex;

        while (factor % 2 == 0 && factor > 1)
        {
            result = result.Scale2X();
            factor /= 2;
        }

        while (factor % 3 == 0 && factor > 1)
        {
            result = result.Scale3X();
            factor /= 3;
        }

        return result;
    }

    public static Texture2D Scale2X(this Texture2D tex)
    {
        var outWidth = tex.width * 2;
        var outHeight = tex.height * 2;

        Texture2D outTex = new(outWidth, outHeight, tex.format, false);
        var pixels = tex.GetPixels();
        var outPixels = new Color[outWidth * outHeight];

        var blank = Color.clear;

        for (var y = 0; y < tex.height; ++y)
        {
            for (var x = 0; x < tex.width; ++x)
            {
                var E = pixels[x + y * tex.width];

                var B = y > 0 ? pixels[x + (y - 1) * tex.width] : blank;
                var D = x > 0 ? pixels[x - 1 + y * tex.width] : blank;
                var F = x < tex.width - 1 ? pixels[x + 1 + y * tex.width] : blank;
                var H = y < tex.height - 1 ? pixels[x + (y + 1) * tex.width] : blank;

                var nX0 = x * 2;
                var nX1 = nX0 + 1;
                var nY0 = y * 2 * outWidth;
                var nY1 = (y * 2 + 1) * outWidth;

                if (B != H && D != F)
                {
                    outPixels[nX0 + nY0] = D == B ? D : E;
                    outPixels[nX1 + nY0] = B == F ? F : E;
                    outPixels[nX0 + nY1] = D == H ? D : E;
                    outPixels[nX1 + nY1] = H == F ? F : E;
                }
                else
                {
                    outPixels[nX0 + nY0] = E;
                    outPixels[nX1 + nY0] = E;
                    outPixels[nX0 + nY1] = E;
                    outPixels[nX1 + nY1] = E;
                }
            }
        }

        outTex.SetPixels(outPixels);
        outTex.Apply();
        return outTex;
    }

    public static Texture2D Scale3X(this Texture2D tex)
    {
        var width = tex.width;
        var height = tex.height;
        var outWidth = width * 3;
        var outHeight = height * 3;

        Texture2D outTex = new(outWidth, outHeight, tex.format, false);
        var pixels = tex.GetPixels();
        var outPixels = new Color[outWidth * outHeight];
        var blank = Color.clear;

        for (var y = 0; y < height; ++y)
        {
            var nY0 = (y * 3 + 0) * outWidth;
            var nY1 = (y * 3 + 1) * outWidth;
            var nY2 = (y * 3 + 2) * outWidth;

            var yIndex = y * width;
            var yIndexPrev = (y - 1) * width;
            var yIndexNext = (y + 1) * width;

            for (var x = 0; x < width; ++x)
            {
                // Center pixel
                var E = pixels[x + yIndex];

                // Surrounding pixels with bounds checking
                var A = x > 0 && y > 0 ? pixels[x - 1 + yIndexPrev] : blank;
                var B = y > 0 ? pixels[x + yIndexPrev] : blank;
                var C = x < width - 1 && y > 0 ? pixels[x + 1 + yIndexPrev] : blank;

                var D = x > 0 ? pixels[x - 1 + yIndex] : blank;
                var F = x < width - 1 ? pixels[x + 1 + yIndex] : blank;

                var G = x > 0 && y < height - 1 ? pixels[x - 1 + yIndexNext] : blank;
                var H = y < height - 1 ? pixels[x + yIndexNext] : blank;
                var I = x < width - 1 && y < height - 1 ? pixels[x + 1 + yIndexNext] : blank;

                var nX0 = x * 3;
                var nX1 = nX0 + 1;
                var nX2 = nX0 + 2;

                // Scale3X
                if (B != H && D != F)
                {
                    outPixels[nX0 + nY0] = D == B ? D : E;
                    outPixels[nX1 + nY0] = (D == B && E != C) || (B == F && E != A) ? B : E;
                    outPixels[nX2 + nY0] = B == F ? F : E;

                    outPixels[nX0 + nY1] = (D == B && E != G) || (D == H && E != A) ? D : E;
                    outPixels[nX1 + nY1] = E;
                    outPixels[nX2 + nY1] = (B == F && E != I) || (H == F && E != C) ? F : E;

                    outPixels[nX0 + nY2] = D == H ? D : E;
                    outPixels[nX1 + nY2] = (D == H && E != I) || (H == F && E != G) ? H : E;
                    outPixels[nX2 + nY2] = H == F ? F : E;
                }
                else
                {
                    outPixels[nX0 + nY0] = E;
                    outPixels[nX1 + nY0] = E;
                    outPixels[nX2 + nY0] = E;
                    outPixels[nX0 + nY1] = E;
                    outPixels[nX1 + nY1] = E;
                    outPixels[nX2 + nY1] = E;
                    outPixels[nX0 + nY2] = E;
                    outPixels[nX1 + nY2] = E;
                    outPixels[nX2 + nY2] = E;
                }
            }
        }

        outTex.SetPixels(outPixels);
        outTex.Apply();
        return outTex;
    }
}
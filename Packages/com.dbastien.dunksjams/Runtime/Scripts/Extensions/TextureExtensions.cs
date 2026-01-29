using System;
using System.Collections.Generic;
using UnityEngine;

public static class Texture2DExtensions
{
    public static Texture2D ColorSwap(this Texture2D tex, Dictionary<Color, Color> swaps)
    {
        Texture2D outTex = new(tex.width, tex.height, tex.format, false);
        Color[] pixels = tex.GetPixels();
        Color[] outPixels = new Color[tex.width * tex.height];

        for (var i = 0; i < pixels.Length; ++i)
        {
            Color originalColor = pixels[i];
            if (swaps.TryGetValue(originalColor, out Color newColor))
            {
                newColor.a = originalColor.a;  // Preserve alpha
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
        Color[] pixels = tex.GetPixels();
        Color[] outPixels = new Color[tex.width * tex.height];

        for (var i = 0; i < pixels.Length; ++i)
        {
            Color cOld = pixels[i];

            float rDiff = MathF.Abs(cOld.r - from.r);
            float gDiff = MathF.Abs(cOld.g - from.g);
            float bDiff = MathF.Abs(cOld.b - from.b);

            float diffFactor = (rDiff + gDiff + bDiff) / 3f;

            if (diffFactor >= ignoreThreshold)
                diffFactor = 1f;

            float lerpValue = MathF.Max(1f - diffFactor, 0f);
            Color cNew = Color.Lerp(cOld, to, MathF.Pow(lerpValue, falloff));
            cNew.a = cOld.a; // Preserve alpha?

            outPixels[i] = cNew;
        }

        outTex.SetPixels(outPixels);
        outTex.Apply();
        return outTex;
    }
    
    public static Texture2D ScaleNX(this Texture2D tex, int factor)
    {
        int reduced = factor;
        while (reduced % 2 == 0) reduced /= 2;
        while (reduced % 3 == 0) reduced /= 3;
        if (reduced != 1) throw new ArgumentException("Unsupported scale factor. Factor must be a multiple of 2 or 3.", nameof(factor));
        
        Texture2D result = tex;

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
        int outWidth = tex.width * 2;
        int outHeight = tex.height * 2;

        Texture2D outTex = new(outWidth, outHeight, tex.format, false);
        Color[] pixels = tex.GetPixels();
        Color[] outPixels = new Color[outWidth * outHeight];

        Color blank = Color.clear;

        for (int y = 0; y < tex.height; ++y)
        {
            for (int x = 0; x < tex.width; ++x)
            {
                Color E = pixels[x + y * tex.width];

                Color B = y > 0 ? pixels[x + (y - 1) * tex.width] : blank;
                Color D = x > 0 ? pixels[x - 1 + y * tex.width] : blank;
                Color F = x < tex.width - 1 ? pixels[x + 1 + y * tex.width] : blank;
                Color H = y < tex.height - 1 ? pixels[x + (y + 1) * tex.width] : blank;

                int nX0 = x * 2;
                int nX1 = nX0 + 1;
                int nY0 = y * 2 * outWidth;
                int nY1 = (y * 2 + 1) * outWidth;

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
        int width = tex.width;
        int height = tex.height;
        int outWidth = width * 3;
        int outHeight = height * 3;

        Texture2D outTex = new(outWidth, outHeight, tex.format, false);
        Color[] pixels = tex.GetPixels();
        Color[] outPixels = new Color[outWidth * outHeight];
        Color blank = Color.clear;

        for (var y = 0; y < height; ++y)
        {
            int nY0 = (y * 3 + 0) * outWidth;
            int nY1 = (y * 3 + 1) * outWidth;
            int nY2 = (y * 3 + 2) * outWidth;

            int yIndex = y * width;
            int yIndexPrev = (y - 1) * width;
            int yIndexNext = (y + 1) * width;

            for (int x = 0; x < width; ++x)
            {
                // Center pixel
                Color E = pixels[x + yIndex];

                // Surrounding pixels with bounds checking
                Color A = x > 0 && y > 0 ? pixels[x - 1 + yIndexPrev] : blank;
                Color B = y > 0 ? pixels[x + yIndexPrev] : blank;
                Color C = x < width - 1 && y > 0 ? pixels[x + 1 + yIndexPrev] : blank;

                Color D = x > 0 ? pixels[x - 1 + yIndex] : blank;
                Color F = x < width - 1 ? pixels[x + 1 + yIndex] : blank;

                Color G = x > 0 && y < height - 1 ? pixels[x - 1 + yIndexNext] : blank;
                Color H = y < height - 1 ? pixels[x + yIndexNext] : blank;
                Color I = x < width - 1 && y < height - 1 ? pixels[x + 1 + yIndexNext] : blank;

                int nX0 = x * 3;
                int nX1 = nX0 + 1;
                int nX2 = nX0 + 2;

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
using System.Collections.Generic;
using UnityEngine;

public static class TextureUtils
{
    static readonly Dictionary<Color, Texture2D> _colorCache = new();

    public static Texture2D Composite(Vector2Int size, Dictionary<Vector2Int, Texture2D> textures)
    {
        Texture2D outTex = new(size.x, size.y, TextureFormat.RGBA32, false);
        var outPixels = new Color[size.x * size.y];

        foreach (var kvp in textures)
        {
            var pos = kvp.Key;
            var texture = kvp.Value;
            var pixels = texture.GetPixels();

            for (var y = 0; y < texture.height; ++y)
            {
                for (var x = 0; x < texture.width; ++x)
                {
                    var outIndex = pos.x + x + (pos.y + y) * size.x;
                    outPixels[outIndex] = pixels[x + y * texture.width];
                }
            }
        }

        outTex.SetPixels(outPixels);
        outTex.Apply();
        return outTex;
    }

    public static Texture2D GetSolidColor(Color color, int width = 2, int height = 2)
    {
        if (_colorCache.TryGetValue(color, out var cachedTex)) return cachedTex;

        var tex = new Texture2D(width, height) { hideFlags = HideFlags.DontSave };
        var pixels = new Color[width * height];
        for (var i = 0; i < pixels.Length; ++i) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        _colorCache[color] = tex;
        return tex;
    }

    // ========================================================================
    // Blur Methods
    // ========================================================================

    /// <summary>Blur a Texture2D using a box filter algorithm.</summary>
    public static Texture2D BoxBlur(Texture2D input, int iterations)
    {
        if (iterations <= 0) return input;

        int w = input.width, h = input.height;
        var output = new Texture2D(w, h);
        var reference = Object.Instantiate(input);
        var rim = new Color[8];

        for (int i = 0; i < iterations; i++)
        {
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var c = reference.GetPixel(x, y);
                    rim[0] = (x > 0 && y > 0) ? reference.GetPixel(x - 1, y - 1) : c;
                    rim[1] = (y > 0) ? reference.GetPixel(x, y - 1) : c;
                    rim[2] = (x < w - 1 && y > 0) ? reference.GetPixel(x + 1, y - 1) : c;
                    rim[3] = (x > 0) ? reference.GetPixel(x - 1, y) : c;
                    rim[4] = (x < w - 1) ? reference.GetPixel(x + 1, y) : c;
                    rim[5] = (x > 0 && y < h - 1) ? reference.GetPixel(x - 1, y + 1) : c;
                    rim[6] = (y < h - 1) ? reference.GetPixel(x, y + 1) : c;
                    rim[7] = (x < w - 1 && y < h - 1) ? reference.GetPixel(x + 1, y + 1) : c;
                    output.SetPixel(x, y, (rim[0] + rim[1] + rim[2] + rim[3] + rim[4] + rim[5] + rim[6] + rim[7]) / 8f);
                }
            }
            Object.Destroy(reference);
            reference = Object.Instantiate(output);
        }
        Object.Destroy(reference);
        output.Apply();
        return output;
    }

    /// <summary>Blur a texture using a Gaussian blur. blurQuality: 1-7, blurStrength: 0-1.</summary>
    public static void GaussianBlur(Texture2D texture, int blurQuality, float blurStrength)
    {
        if (texture == null || blurQuality < 1 || blurQuality > 7 || blurStrength < 0f || blurStrength > 1f) return;

        int w = texture.width, h = texture.height;
        float blurAmount = blurStrength;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float denominator = 0f;
                Color blurred = Color.clear;

                for (int bx = -blurQuality; bx <= blurQuality; bx++)
                {
                    for (int by = -blurQuality; by <= blurQuality; by++)
                    {
                        int sx = (int)(x + bx * blurAmount);
                        int sy = (int)(y + by * blurAmount);
                        if (sx >= 0 && sx < w && sy >= 0 && sy < h)
                        {
                            float gd = GaussianDistribution(bx, 7f);
                            blurred += texture.GetPixel(sx, sy) * gd;
                            denominator += gd;
                        }
                    }
                }

                if (denominator >= 0.001f)
                    texture.SetPixel(x, y, blurred / denominator);
            }
        }
        texture.Apply();
    }

    /// <summary>Gaussian probability distribution function.</summary>
    public static float GaussianDistribution(float x, float sigma) =>
        0.39894f * Mathf.Exp(-0.5f * x * x / (sigma * sigma)) / sigma;

    // ========================================================================
    // Transform Methods
    // ========================================================================

    /// <summary>Flip a texture top-to-bottom.</summary>
    public static Texture2D FlipTexture(Texture2D input)
    {
        if (input == null) return null;
        int w = input.width, h = input.height;
        var flipped = new Texture2D(w, h, input.format, input.mipmapCount > 1);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                flipped.SetPixel(x, y, input.GetPixel(x, h - 1 - y));
        flipped.Apply();
        return flipped;
    }

    /// <summary>Rotate a texture by an angle in degrees.</summary>
    public static Texture2D RotateTexture(Texture2D input, float angle)
    {
        if (input == null) return null;
        int w = input.width, h = input.height;
        var rotated = new Texture2D(w, h);
        rotated.name = input.name;

        angle += 90f; // correct rotation offset
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);

        float x0 = (-w / 2f) * cos + (-h / 2f) * (-sin) + w / 2f;
        float y0 = (-w / 2f) * sin + (-h / 2f) * cos + h / 2f;

        for (int x = 0; x < w; x++)
        {
            float x2 = x0, y2 = y0;
            for (int y = 0; y < h; y++)
            {
                x2 += cos; y2 += sin;
                int px = Mathf.FloorToInt(x2), py = Mathf.FloorToInt(y2);
                rotated.SetPixel(x, y, (px >= 0 && px < w && py >= 0 && py < h) ? input.GetPixel(px, py) : Color.clear);
            }
            x0 += -sin; y0 += cos;
        }
        rotated.Apply();
        return rotated;
    }

    /// <summary>Tint a texture with a colour and return a new texture.</summary>
    public static Texture2D TintTexture(Texture2D input, Color tint, float strength)
    {
        if (input == null) return null;
        var tinted = Object.Instantiate(input);
        tinted.name = input.name;
        int mipCount = Mathf.Max(1, tinted.mipmapCount);
        for (int mip = 0; mip < mipCount; mip++)
        {
            var colors = tinted.GetPixels(mip);
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.Lerp(colors[i], tint, strength);
            tinted.SetPixels(colors, mip);
        }
        tinted.Apply();
        return tinted;
    }

    // ========================================================================
    // Normal Map
    // ========================================================================

    /// <summary>Create a height-to-normal map using a curve modifier.</summary>
    public static Texture2D NormalMapFromHeightmap(Texture2D input, AnimationCurve curveModifier)
    {
        if (input == null) return null;
        int w = input.width, h = input.height;
        var output = new Texture2D(w, h);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                output.SetPixel(x, y, curveModifier.Evaluate(input.GetPixel(x, y).grayscale) * Color.white);
        output.Apply();
        return output;
    }

    // ========================================================================
    // Color Math
    // ========================================================================

    /// <summary>Inverse lerp between two colours per channel.</summary>
    public static Color InverseLerpColor(Color a, Color b, Color value) => new(
        Mathf.InverseLerp(a.r, b.r, value.r),
        Mathf.InverseLerp(a.g, b.g, value.g),
        Mathf.InverseLerp(a.b, b.b, value.b),
        Mathf.InverseLerp(a.a, b.a, value.a)
    );

    /// <summary>Per-channel max of two colours.</summary>
    public static Color MaxColor(Color a, Color b) => new(
        Mathf.Max(a.r, b.r), Mathf.Max(a.g, b.g), Mathf.Max(a.b, b.b), Mathf.Max(a.a, b.a)
    );

    /// <summary>Per-channel colour difference (absolute).</summary>
    public static Color ColorDifference(Color a, Color b) => new(
        Mathf.Abs(a.r - b.r), Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b), Mathf.Abs(a.a - b.a)
    );

    /// <summary>Divide colour a by colour b per channel (safe, avoids div-by-zero).</summary>
    public static Color DivideColor(Color a, Color b) => new(
        b.r > 0.0001f ? a.r / b.r : 0f,
        b.g > 0.0001f ? a.g / b.g : 0f,
        b.b > 0.0001f ? a.b / b.b : 0f,
        b.a > 0.0001f ? a.a / b.a : 0f
    );

    /// <summary>Unclamped lerp between two colours.</summary>
    public static Color ColorLerpUnclamped(Color a, Color b, float t) => new(
        a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t
    );

    // ========================================================================
    // RenderTexture Helpers
    // ========================================================================

    /// <summary>Create a RenderTexture with standard settings.</summary>
    public static RenderTexture CreateRenderTexture(int width, int height, int depth = 24,
        RenderTextureFormat format = RenderTextureFormat.ARGB32)
    {
        var rt = new RenderTexture(width, height, depth, format);
        rt.Create();
        return rt;
    }

    /// <summary>Destroy a RenderTexture and release its resources.</summary>
    public static void DestroyRenderTexture(ref RenderTexture rt)
    {
        if (rt == null) return;
        rt.Release();
        Object.Destroy(rt);
        rt = null;
    }

    // ========================================================================
    // Drawing Methods
    // ========================================================================

    /// <summary>Draw a solid filled circle on a texture.</summary>
    public static void DrawCircleSolid(Texture2D texture, int cx, int cy, int radius, Color color)
    {
        if (texture == null) return;
        int w = texture.width, h = texture.height;
        int r2 = radius * radius;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= r2)
                {
                    int px = cx + x, py = cy + y;
                    if (px >= 0 && px < w && py >= 0 && py < h)
                        texture.SetPixel(px, py, color);
                }
            }
        }
    }

    /// <summary>Draw a circle with gradient falloff on a texture.</summary>
    public static void DrawCircleGradient(Texture2D texture, int cx, int cy, int radius, Color color)
    {
        if (texture == null) return;
        int w = texture.width, h = texture.height;
        int r2 = radius * radius;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int dist2 = x * x + y * y;
                if (dist2 <= r2)
                {
                    int px = cx + x, py = cy + y;
                    if (px >= 0 && px < w && py >= 0 && py < h)
                    {
                        float t = 1f - Mathf.Sqrt(dist2) / radius;
                        var existing = texture.GetPixel(px, py);
                        texture.SetPixel(px, py, Color.Lerp(existing, color, t));
                    }
                }
            }
        }
    }

    /// <summary>Subtract a circle (reduce alpha) on a texture.</summary>
    public static void DrawCircleSubtract(Texture2D texture, int cx, int cy, int radius, float strength)
    {
        if (texture == null) return;
        int w = texture.width, h = texture.height;
        int r2 = radius * radius;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int dist2 = x * x + y * y;
                if (dist2 <= r2)
                {
                    int px = cx + x, py = cy + y;
                    if (px >= 0 && px < w && py >= 0 && py < h)
                    {
                        float t = 1f - Mathf.Sqrt(dist2) / radius;
                        var c = texture.GetPixel(px, py);
                        c.a = Mathf.Max(0f, c.a - t * strength);
                        texture.SetPixel(px, py, c);
                    }
                }
            }
        }
    }

    /// <summary>Draw a smooth (C2 falloff) circle on a texture.</summary>
    public static void SmoothCircle(Texture2D texture, int cx, int cy, int radius, Color color)
    {
        if (texture == null) return;
        int w = texture.width, h = texture.height;
        int r2 = radius * radius;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int dist2 = x * x + y * y;
                if (dist2 <= r2)
                {
                    int px = cx + x, py = cy + y;
                    if (px >= 0 && px < w && py >= 0 && py < h)
                    {
                        float nd = (float)dist2 / r2;
                        float falloff = 1f - nd;
                        falloff = falloff * falloff * falloff; // C2 falloff
                        var existing = texture.GetPixel(px, py);
                        texture.SetPixel(px, py, Color.Lerp(existing, color, falloff));
                    }
                }
            }
        }
    }

    // ========================================================================
    // Blending / Combining
    // ========================================================================

    /// <summary>Combine two textures additively.</summary>
    public static Texture2D CombineAdditive(Texture2D a, Texture2D b)
    {
        if (a == null || b == null) return a;
        int w = Mathf.Min(a.width, b.width), h = Mathf.Min(a.height, b.height);
        var result = new Texture2D(w, h);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result.SetPixel(x, y, a.GetPixel(x, y) + b.GetPixel(x, y));
        result.Apply();
        return result;
    }

    /// <summary>Per-pixel maximum of two textures.</summary>
    public static Texture2D Maximum(Texture2D a, Texture2D b)
    {
        if (a == null || b == null) return a;
        int w = Mathf.Min(a.width, b.width), h = Mathf.Min(a.height, b.height);
        var result = new Texture2D(w, h);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result.SetPixel(x, y, MaxColor(a.GetPixel(x, y), b.GetPixel(x, y)));
        result.Apply();
        return result;
    }

    /// <summary>Per-pixel minimum of two textures.</summary>
    public static Texture2D Minimum(Texture2D a, Texture2D b)
    {
        if (a == null || b == null) return a;
        int w = Mathf.Min(a.width, b.width), h = Mathf.Min(a.height, b.height);
        var result = new Texture2D(w, h);
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var ca = a.GetPixel(x, y);
                var cb = b.GetPixel(x, y);
                result.SetPixel(x, y, new Color(
                    Mathf.Min(ca.r, cb.r), Mathf.Min(ca.g, cb.g), Mathf.Min(ca.b, cb.b), Mathf.Min(ca.a, cb.a)));
            }
        }
        result.Apply();
        return result;
    }

    // ========================================================================
    // Scaling / Cropping
    // ========================================================================

    /// <summary>Generate a downscaled texture by the given factor (e.g., 2 = half size).</summary>
    public static Texture2D GenerateDownscaled(Texture2D input, int factor)
    {
        if (input == null || factor < 1) return input;
        int nw = input.width / factor, nh = input.height / factor;
        if (nw < 1 || nh < 1) return input;
        var result = new Texture2D(nw, nh);
        for (int x = 0; x < nw; x++)
            for (int y = 0; y < nh; y++)
                result.SetPixel(x, y, input.GetPixel(x * factor, y * factor));
        result.Apply();
        return result;
    }

    /// <summary>Point-scale a texture to a new size.</summary>
    public static Texture2D PointScale(Texture2D input, int newWidth, int newHeight)
    {
        if (input == null) return null;
        var result = new Texture2D(newWidth, newHeight);
        float xRatio = (float)input.width / newWidth;
        float yRatio = (float)input.height / newHeight;
        for (int x = 0; x < newWidth; x++)
            for (int y = 0; y < newHeight; y++)
                result.SetPixel(x, y, input.GetPixel((int)(x * xRatio), (int)(y * yRatio)));
        result.Apply();
        return result;
    }

    /// <summary>Extract a sub-region from a texture.</summary>
    public static Texture2D CropTexture(Texture2D input, int startX, int startY, int cropWidth, int cropHeight)
    {
        if (input == null) return null;
        var result = new Texture2D(cropWidth, cropHeight);
        for (int x = 0; x < cropWidth; x++)
            for (int y = 0; y < cropHeight; y++)
                result.SetPixel(x, y, input.GetPixel(startX + x, startY + y));
        result.Apply();
        return result;
    }

    // ========================================================================
    // Sampling
    // ========================================================================

    /// <summary>Bilinear sample a texture at normalized coordinates (0-1).</summary>
    public static Color SampleBilinear(Texture2D texture, float u, float v)
    {
        if (texture == null) return Color.clear;
        int w = texture.width, h = texture.height;
        float x = u * (w - 1), y = v * (h - 1);
        int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
        int x1 = Mathf.Min(x0 + 1, w - 1), y1 = Mathf.Min(y0 + 1, h - 1);
        float fx = x - x0, fy = y - y0;
        var c00 = texture.GetPixel(x0, y0);
        var c10 = texture.GetPixel(x1, y0);
        var c01 = texture.GetPixel(x0, y1);
        var c11 = texture.GetPixel(x1, y1);
        return Color.Lerp(Color.Lerp(c00, c10, fx), Color.Lerp(c01, c11, fx), fy);
    }

    // ========================================================================
    // Channel Writes
    // ========================================================================

    /// <summary>Set the RGB channels of a texture from a source, keeping alpha.</summary>
    public static void SetTextureRGB(Texture2D target, Texture2D source)
    {
        if (target == null || source == null) return;
        int w = Mathf.Min(target.width, source.width), h = Mathf.Min(target.height, source.height);
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var src = source.GetPixel(x, y);
                var dst = target.GetPixel(x, y);
                target.SetPixel(x, y, new Color(src.r, src.g, src.b, dst.a));
            }
        }
        target.Apply();
    }

    /// <summary>Set the alpha channel of a texture from a grayscale source.</summary>
    public static void SetTextureAlpha(Texture2D target, Texture2D alphaSource)
    {
        if (target == null || alphaSource == null) return;
        int w = Mathf.Min(target.width, alphaSource.width), h = Mathf.Min(target.height, alphaSource.height);
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var dst = target.GetPixel(x, y);
                dst.a = alphaSource.GetPixel(x, y).grayscale;
                target.SetPixel(x, y, dst);
            }
        }
        target.Apply();
    }

    // ========================================================================
    // Gradient Modifier
    // ========================================================================

    /// <summary>Modify a texture by applying a vertical gradient multiplier.</summary>
    public static void ModifyWithVerticalGradient(Texture2D texture, Gradient gradient)
    {
        if (texture == null || gradient == null) return;
        int w = texture.width, h = texture.height;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / (h - 1);
                texture.SetPixel(x, y, texture.GetPixel(x, y) * gradient.Evaluate(t));
            }
        }
        texture.Apply();
    }
}
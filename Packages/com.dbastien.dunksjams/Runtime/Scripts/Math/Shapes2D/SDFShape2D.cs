using System;
using UnityEngine;

public static class ShapeSdfBaker2D
{
    public struct Settings
    {
        public int Width;
        public int Height;

        public Rect WorldRect;

        public float MaxDistanceWorld;

        public bool NormalizeTo01;
        public bool InsideIsBlack;
        public bool UseRFloatIfPossible;

        public static Settings Default(Rect worldRect, int size = 256, float maxDistanceWorld = 0.25f)
        {
            return new Settings
            {
                Width = size,
                Height = size,
                WorldRect = worldRect,
                MaxDistanceWorld = Mathf.Max(1e-6f, maxDistanceWorld),
                NormalizeTo01 = true,
                InsideIsBlack = true,
                UseRFloatIfPossible = true
            };
        }
    }

    public static Texture2D Bake(IShape2D shape, Settings s)
    {
        int w = Mathf.Max(1, s.Width);
        int h = Mathf.Max(1, s.Height);

        var distances = new float[w * h];

        float invW = 1f / w;
        float invH = 1f / h;

        float xMin = s.WorldRect.xMin;
        float yMin = s.WorldRect.yMin;
        float xSize = s.WorldRect.width;
        float ySize = s.WorldRect.height;

        float maxD = Mathf.Max(1e-6f, s.MaxDistanceWorld);
        float invMaxD = 1f / maxD;

        for (int y = 0; y < h; y++)
        {
            float v = (y + 0.5f) * invH;
            float wy = yMin + v * ySize;

            int row = y * w;
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) * invW;
                float wx = xMin + u * xSize;

                Vector2 p = new Vector2(wx, wy);

                Vector2 nearest = shape.NearestPoint(p);
                float d = (nearest - p).magnitude;

                float signed = shape.Contains(p) ? -d : d;

                float value;
                if (s.NormalizeTo01)
                {
                    float t = Mathf.Clamp(signed * invMaxD, -1f, 1f);
                    float n = 0.5f + 0.5f * t;
                    value = s.InsideIsBlack ? n : (1f - n);
                }
                else
                {
                    value = signed;
                }

                distances[row + x] = value;
            }
        }

        return CreateTexture(distances, w, h, s.NormalizeTo01, s.UseRFloatIfPossible);
    }

    static Texture2D CreateTexture(float[] data, int w, int h, bool normalized01, bool useRFloatIfPossible)
    {
        Texture2D tex = null;

        if (useRFloatIfPossible && normalized01)
        {
            try
            {
                tex = new Texture2D(w, h, TextureFormat.RFloat, mipChain: false, linear: true);
                tex.SetPixelData(data, 0);
                tex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                return tex;
            }
            catch
            {
                if (tex != null) UnityEngine.Object.Destroy(tex);
                tex = null;
            }
        }

        var colors = new Color[w * h];
        for (int i = 0; i < data.Length; i++)
        {
            float v = normalized01 ? data[i] : Mathf.Clamp01(0.5f + 0.5f * data[i]);
            colors[i] = new Color(v, v, v, 1f);
        }

        tex = new Texture2D(w, h, TextureFormat.RGBA32, mipChain: false, linear: true);
        tex.SetPixels(colors);
        tex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }
}
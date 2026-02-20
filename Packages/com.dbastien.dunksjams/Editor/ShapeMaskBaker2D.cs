using UnityEngine;

public static class ShapeMaskBaker2D
{
    public enum FillMode
    {
        Solid,
        Stroke
    }

    public enum ProgressMode
    {
        None,
        LinearX,
        LinearY,
        Radial
    }

    public struct Settings
    {
        public int Width;
        public int Height;

        public Rect WorldRect;

        public FillMode Fill;
        public float StrokeWidthWorld;
        public bool StrokeOnlyInside;

        public float FeatherWorld;

        public ProgressMode Progress;
        public float FillAmount01;

        public float RadialStartRadians;
        public bool RadialClockwise;

        public bool UseR8IfPossible;

        public static Settings Default(Rect worldRect, int size = 256)
        {
            return new Settings
            {
                Width = size,
                Height = size,
                WorldRect = worldRect,

                Fill = FillMode.Solid,
                StrokeWidthWorld = 0.05f,
                StrokeOnlyInside = false,

                FeatherWorld = 0.01f,

                Progress = ProgressMode.None,
                FillAmount01 = 1f,

                RadialStartRadians = 0f,
                RadialClockwise = false,

                UseR8IfPossible = true
            };
        }
    }

    public static Texture2D BakeMask(IShape2D shape, Settings s)
    {
        int w = Mathf.Max(1, s.Width);
        int h = Mathf.Max(1, s.Height);

        float feather = Mathf.Max(1e-6f, s.FeatherWorld);
        float strokeWidth = Mathf.Max(0f, s.StrokeWidthWorld);
        float strokeHalf = strokeWidth * 0.5f;

        var alpha = new byte[w * h];

        float invW = 1f / w;
        float invH = 1f / h;

        float xMin = s.WorldRect.xMin;
        float yMin = s.WorldRect.yMin;
        float xSize = s.WorldRect.width;
        float ySize = s.WorldRect.height;

        float fillT = Mathf.Clamp01(s.FillAmount01);
        float radialSweep = fillT * Mathf.PI * 2f;

        bool useProgress = s.Progress != ProgressMode.None && fillT < 1f;

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
                float dist = (nearest - p).magnitude;
                bool inside = shape.Contains(p);

                float a = s.Fill switch
                {
                    FillMode.Solid => SolidFillAlpha(inside, dist, feather),
                    FillMode.Stroke => StrokeAlpha(inside, dist, strokeHalf, feather, s.StrokeOnlyInside),
                    _ => 0f
                };

                if (a > 0f && useProgress)
                    a *= ProgressMask(u, v, p, s, radialSweep);

                alpha[row + x] = (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f);
            }
        }

        return CreateAlphaTexture(alpha, w, h, s.UseR8IfPossible);
    }

    static float SolidFillAlpha(bool inside, float distToBoundary, float featherWorld)
    {
        if (!inside) return 0f;

        float f = Mathf.Max(1e-6f, featherWorld);
        float edge = 1f - Smoothstep(0f, f, distToBoundary);
        return edge;
    }

    static float StrokeAlpha(bool inside, float distToBoundary, float strokeHalfWorld, float featherWorld, bool onlyInside)
    {
        if (strokeHalfWorld <= 1e-8f) return 0f;
        if (onlyInside && !inside) return 0f;

        float f = Mathf.Max(1e-6f, featherWorld);
        float w = Mathf.Max(1e-6f, strokeHalfWorld);

        float a = 1f - Smoothstep(w - f, w + f, distToBoundary);
        return a;
    }

    static float Smoothstep(float a, float b, float x)
    {
        float denom = Mathf.Max(1e-6f, b - a);
        float t = Mathf.Clamp01((x - a) / denom);
        return t * t * (3f - 2f * t);
    }

    static float ProgressMask(float u, float v, Vector2 pWorld, Settings s, float radialSweep)
    {
        float t = Mathf.Clamp01(s.FillAmount01);

        return s.Progress switch
        {
            ProgressMode.LinearX => u <= t ? 1f : 0f,
            ProgressMode.LinearY => v <= t ? 1f : 0f,
            ProgressMode.Radial => RadialMask(pWorld, s, radialSweep),
            _ => 1f
        };
    }

    static float RadialMask(Vector2 pWorld, Settings s, float sweep)
    {
        Vector2 c = s.WorldRect.center;
        Vector2 d = pWorld - c;

        if (d.sqrMagnitude <= 1e-12f) return 1f;

        float angle = Mathf.Atan2(d.y, d.x);
        float a0 = WrapAngle(angle - s.RadialStartRadians);

        float t = s.RadialClockwise ? WrapAngle(-a0) : a0;
        return t <= sweep ? 1f : 0f;
    }

    static float WrapAngle(float a)
    {
        float twoPi = Mathf.PI * 2f;
        a %= twoPi;
        if (a < 0f) a += twoPi;
        return a;
    }

    static Texture2D CreateAlphaTexture(byte[] alpha, int w, int h, bool tryR8)
    {
        Texture2D tex = null;

        if (tryR8)
        {
            try
            {
                tex = new Texture2D(w, h, TextureFormat.R8, mipChain: false, linear: true);
                tex.SetPixelData(alpha, 0);
                tex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                return tex;
            }
            catch
            {
                if (tex != null) Object.Destroy(tex);
                tex = null;
            }
        }

        var colors = new Color32[w * h];
        for (int i = 0; i < alpha.Length; i++)
            colors[i] = new Color32(255, 255, 255, alpha[i]);

        tex = new Texture2D(w, h, TextureFormat.RGBA32, mipChain: false, linear: true);
        tex.SetPixels32(colors);
        tex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }
}
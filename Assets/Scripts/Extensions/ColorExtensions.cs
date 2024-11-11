using UnityEngine;

public static class ColorExtensions
{
    public static Color LerpUnclamped(this Color l, Color r, float t) => l + t * (r - l);
    public static Color PremultiplyAlpha(this Color c) => new(c.r * c.a, c.g * c.a, c.b * c.a, c.a);
}

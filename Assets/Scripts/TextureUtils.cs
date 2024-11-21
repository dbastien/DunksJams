using System.Collections.Generic;
using UnityEngine;

public static class TextureUtils
{
    static readonly Dictionary<Color, Texture2D> _colorCache = new();
    
    public static Texture2D Composite(Vector2Int size, Dictionary<Vector2Int, Texture2D> textures)
    {
        Texture2D outTex = new(size.x, size.y, TextureFormat.RGBA32, false);
        Color[] outPixels = new Color[size.x * size.y];

        foreach (KeyValuePair<Vector2Int, Texture2D> kvp in textures)
        {
            Vector2Int pos = kvp.Key;
            Texture2D texture = kvp.Value;
            Color[] pixels = texture.GetPixels();

            for (var y = 0; y < texture.height; ++y)
            {
                for (var x = 0; x < texture.width; ++x)
                {
                    int outIndex = pos.x + x + (pos.y + y) * size.x;
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
        for (int i = 0; i < pixels.Length; ++i) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        
        _colorCache[color] = tex;
        return tex;
    }
}

using UnityEngine;

public static class GUIStyleExtensions
{
    public static Vector2 CalcSize(this GUIStyle g, params GUIContent[] items)
    {
        float width = 0f, height = 0f;
        foreach (var item in items)
        {
            var size = g.CalcSize(item);
            width = Mathf.Max(width, size.x);
            height += size.y;
        }
        return new(width, height);
    }
    
    public static Vector2 CalcVerticalSize(this GUIStyle g, params GUIContent[] items)
    {
        float paddingHeight = g.padding.vertical;
        float maxWidth = 0, totalHeight = paddingHeight * (items.Length - 1);
        foreach (var item in items)
        {
            var size = g.CalcSize(item);
            totalHeight += size.y;
            maxWidth = Mathf.Max(maxWidth, size.x + g.padding.horizontal);
        }
        return new(maxWidth, totalHeight);
    }

    public static Vector2 CalcHorizontalSize(this GUIStyle g, params GUIContent[] items)
    {
        float paddingWidth = g.padding.horizontal;
        float totalWidth = paddingWidth * (items.Length - 1);
        float maxHeight = 0;
        foreach (var item in items)
        {
            var size = g.CalcSize(item);
            totalWidth += size.x;
            maxHeight = Mathf.Max(maxHeight, size.y + g.padding.vertical);
        }
        return new(totalWidth, maxHeight);
    }

    public static GUIStyle CloneWithPadding(this GUIStyle g, RectOffset padding, RectOffset margin = null) =>
        new(g) { padding = padding, margin = margin ?? g.margin };

    public static void FadeTextColor(this GUIStyle g, Color startColor, Color endColor, float duration) =>
        g.normal.textColor = Color.Lerp(startColor, endColor, Mathf.PingPong(Time.time / duration, 1f));
    
    public static void WithFontSize(this GUIStyle g, int fontSize, System.Action drawAction)
    {
        int originalSize = g.fontSize;
        g.fontSize = fontSize;
        drawAction?.Invoke();
        g.fontSize = originalSize;
    }
    
    public static void WithFontStyle(this GUIStyle g, FontStyle fontStyle, System.Action drawAction)
    {
        var originalStyle = g.fontStyle;
        g.fontStyle = fontStyle;
        drawAction?.Invoke();
        g.fontStyle = originalStyle;
    }

    public static GUIStyle SetAlignment(this GUIStyle g, TextAnchor alignment)
    {
        g.alignment = alignment;
        return g;
    }

    public static int AdjustFontSizeToFitWidth(this GUIStyle g, GUIContent content, float maxWidth, int minSize = 8, int maxSize = 40)
    {
        for (int fontSize = maxSize; fontSize >= minSize; --fontSize)
        {
            g.fontSize = fontSize;
            if (g.CalcSize(content).x <= maxWidth) return fontSize;
        }
        return minSize;
    }
    
    public static GUIStyle WithBackground(this GUIStyle g, Texture2D tex) { g.normal.background = tex; return g; }
    
    public static GUIStyle CreateStyle(this GUIStyle g, Font font = null, int fontSize = 12, int pad = 2) =>
        new(g) { font = font, fontSize = fontSize, padding = new(pad, pad, pad, pad) };

    public static GUIStyle CreateStyle(this GUIStyle g, int pad, int margin) =>
        new(g) { padding = new(pad, pad, pad, pad), margin = new(margin, margin, margin, margin) };
}

using UnityEngine;
using UnityEngine.UIElements;

public static class IStyleExtensions
{
    public static IStyle SetBorderColor(this IStyle s, Color color)
    {
        s.borderTopColor = color;
        s.borderLeftColor = color;
        s.borderRightColor = color;
        s.borderBottomColor = color;
        return s;
    }

    public static IStyle SetBorderWidth(this IStyle s, float width) => s.SetBorderWidth(width, width, width, width);

    public static IStyle SetBorderWidth(this IStyle s, float? top = null, float? left = null, float? right = null, float? bottom = null)
    {
        if (top.HasValue) s.borderTopWidth = top.Value;
        if (left.HasValue) s.borderLeftWidth = left.Value;
        if (right.HasValue) s.borderRightWidth = right.Value;
        if (bottom.HasValue) s.borderBottomWidth = bottom.Value;
        return s;
    }

    public static IStyle ClearBorderWidth(this IStyle s)
    {
        s.borderTopWidth = StyleKeyword.Null;
        s.borderLeftWidth = StyleKeyword.Null;
        s.borderRightWidth = StyleKeyword.Null;
        s.borderBottomWidth = StyleKeyword.Null;
        return s;
    }

    public static IStyle SetBorderRadius(this IStyle s, float radius) => s.SetBorderRadius(radius, radius, radius, radius);

    public static IStyle SetBorderRadius(this IStyle s, float? top = null, float? left = null, float? right = null, float? bottom = null)
    {
        if (top.HasValue) s.borderTopLeftRadius = top.Value;
        if (left.HasValue) s.borderTopRightRadius = left.Value;
        if (right.HasValue) s.borderBottomLeftRadius = right.Value;
        if (bottom.HasValue) s.borderBottomRightRadius = bottom.Value;
        return s;
    }

    public static IStyle ClearBorderRadius(this IStyle s)
    {
        s.borderTopLeftRadius = StyleKeyword.Null;
        s.borderTopRightRadius = StyleKeyword.Null;
        s.borderBottomLeftRadius = StyleKeyword.Null;
        s.borderBottomRightRadius = StyleKeyword.Null;
        return s;
    }

    public static IStyle SetMargin(this IStyle s, float length) => s.SetMargin(length, length, length, length);

    public static IStyle SetMargin(this IStyle s, float? top = null, float? left = null, float? right = null, float? bottom = null)
    {
        if (top.HasValue) s.marginTop = top.Value;
        if (left.HasValue) s.marginLeft = left.Value;
        if (right.HasValue) s.marginRight = right.Value;
        if (bottom.HasValue) s.marginBottom = bottom.Value;
        return s;
    }

    public static IStyle ClearMargin(this IStyle s)
    {
        s.marginTop = StyleKeyword.Null;
        s.marginLeft = StyleKeyword.Null;
        s.marginRight = StyleKeyword.Null;
        s.marginBottom = StyleKeyword.Null;
        return s;
    }

    public static IStyle SetPadding(this IStyle s, float length) => s.SetPadding(length, length, length, length);

    public static IStyle SetPadding(this IStyle s, float? top = null, float? left = null, float? right = null, float? bottom = null)
    {
        if (top.HasValue) s.paddingTop = top.Value;
        if (left.HasValue) s.paddingLeft = left.Value;
        if (right.HasValue) s.paddingRight = right.Value;
        if (bottom.HasValue) s.paddingBottom = bottom.Value;
        return s;
    }

    public static IStyle ClearPadding(this IStyle s)
    {
        s.paddingTop = StyleKeyword.Null;
        s.paddingLeft = StyleKeyword.Null;
        s.paddingRight = StyleKeyword.Null;
        s.paddingBottom = StyleKeyword.Null;
        return s;
    }

    public static IStyle SetPosition(this IStyle s, Position? type = null, float? top = null, float? left = null, float? right = null, float? bottom = null)
    {
        if (type.HasValue) s.position = type.Value;
        if (top.HasValue) s.top = top.Value;
        if (left.HasValue) s.left = left.Value;
        if (right.HasValue) s.right = right.Value;
        if (bottom.HasValue) s.bottom = bottom.Value;
        return s;
    }

    public static IStyle ClearPosition(this IStyle s)
    {
        s.top = StyleKeyword.Null;
        s.left = StyleKeyword.Null;
        s.right = StyleKeyword.Null;
        s.bottom = StyleKeyword.Null;
        return s;
    }

    public static IStyle SetSize(this IStyle s, float? width = null, float? height = null)
    {
        if (width.HasValue) s.width = width.Value;
        if (height.HasValue) s.height = height.Value;
        return s;
    }

    public static IStyle ClearSize(this IStyle s)
    {
        s.width = StyleKeyword.Null;
        s.height = StyleKeyword.Null;
        return s;
    }

    public static IStyle SetSlice(this IStyle s, int? top = null, int? left = null, int? right = null, int? bottom = null)
    {
        if (top.HasValue) s.unitySliceTop = top.Value;
        if (left.HasValue) s.unitySliceLeft = left.Value;
        if (right.HasValue) s.unitySliceRight = right.Value;
        if (bottom.HasValue) s.unitySliceBottom = bottom.Value;
        return s;
    }
}
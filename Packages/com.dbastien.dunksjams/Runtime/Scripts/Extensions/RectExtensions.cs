using System;
using UnityEditor;
using UnityEngine;

public static class RectExtensions
{
    public static Vector2 TopLeft(this Rect r) => new(r.xMin, r.yMax);
    public static Vector2 TopCenter(this Rect r) => new(r.center.x, r.yMax);
    public static Vector2 TopRight(this Rect r) => new(r.xMax, r.yMax);

    public static Vector2 MiddleLeft(this Rect r) => new(r.xMin, r.center.y);
    public static Vector2 MiddleRight(this Rect r) => new(r.xMax, r.center.y);

    public static Vector2 BottomLeft(this Rect r) => new(r.xMin, r.yMin);
    public static Vector2 BottomCenter(this Rect r) => new(r.center.x, r.yMin);
    public static Vector2 BottomRight(this Rect r) => new(r.xMax, r.yMin);

    public static Rect SetPosition(this Rect r, Vector2 newPos) =>
        new(newPos.x, newPos.y, r.width, r.height);

    public static Rect CenterInParent(this Rect r, Rect parent) =>
        new(parent.xMin + (parent.width - r.width) * 0.5f,
            parent.yMin + (parent.height - r.height) * 0.5f,
            r.width,
            r.height);

    public static Rect ClampInside(this Rect r, Rect bounds) =>
        new(Mathf.Clamp(r.xMin, bounds.xMin, bounds.xMax - r.width),
            Mathf.Clamp(r.yMin, bounds.yMin, bounds.yMax - r.height),
            r.width,
            r.height);

    public static Rect[] Subdivide(this Rect r, int rows, int cols)
    {
        float cellWidth = r.width / cols;
        float cellHeight = r.height / rows;
        var rects = new Rect[rows * cols];
        for (var y = 0; y < rows; ++y)
        for (var x = 0; x < cols; ++x)
            rects[y * cols + x] = new Rect(
                r.xMin + x * cellWidth,
                r.yMin + y * cellHeight,
                cellWidth,
                cellHeight);

        return rects;
    }

    /// <summary> Calculate signed depth of intersect of two rects. </summary>
    public static Vector2 GetIntersectionDepth(this Rect r, Rect other)
    {
        float halfWidthA = r.width * 0.5f;
        float halfHeightA = r.height * 0.5f;
        float halfWidthB = other.width * 0.5f;
        float halfHeightB = other.height * 0.5f;

        // current and min non-intersect distances of centers
        float distX = r.xMin + halfWidthA - (other.xMin + halfWidthB);
        float distY = r.yMin + halfHeightA - (other.yMin + halfHeightB);
        float minDistX = halfWidthA + halfWidthB;
        float minDistY = halfHeightA + halfHeightB;

        // no intersect
        if (MathF.Abs(distX) >= minDistX || MathF.Abs(distY) >= minDistY)
            return Vector2.zero;

        // intersect depth
        float depthX = distX > 0 ? minDistX - distX : -minDistX - distX;
        float depthY = distY > 0 ? minDistY - distY : -minDistY - distY;
        return new Vector2(depthX, depthY);
    }

    public static bool ContainsInclusive(this Rect r, Vector2 point) =>
        point.x >= r.xMin && point.x <= r.xMax && point.y >= r.yMin && point.y <= r.yMax;

    public static Rect? Overlap(this Rect r, Rect other)
    {
        float xMin = Mathf.Max(r.xMin, other.xMin);
        float yMin = Mathf.Max(r.yMin, other.yMin);
        float xMax = Mathf.Min(r.xMax, other.xMax);
        float yMax = Mathf.Min(r.yMax, other.yMax);

        return xMin < xMax && yMin < yMax ? new Rect(xMin, yMin, xMax - xMin, yMax - yMin) : null;
    }

    public static Rect ExpandToFit(this Rect r, Rect other)
    {
        float xMin = Mathf.Min(r.xMin, other.xMin);
        float yMin = Mathf.Min(r.yMin, other.yMin);
        float xMax = Mathf.Max(r.xMax, other.xMax);
        float yMax = Mathf.Max(r.yMax, other.yMax);

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    public static Rect Translate(this Rect r, Vector2 offset) =>
        new(r.xMin + offset.x, r.yMin + offset.y, r.width, r.height);

    public static Rect ResizeByFactor(this Rect r, float scale)
    {
        float newWidth = r.width * scale;
        float newHeight = r.height * scale;
        return new Rect(r.center.x - newWidth / 2, r.center.y - newHeight / 2, newWidth, newHeight);
    }

    public static Rect ResizeByPadding(this Rect r, float amount) =>
        new(r.xMin - amount, r.yMin - amount, r.width + 2 * amount, r.height + 2 * amount);

    public static Rect Lerp(Rect a, Rect b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Rect(
            Mathf.Lerp(a.x, b.x, t),
            Mathf.Lerp(a.y, b.y, t),
            Mathf.Lerp(a.width, b.width, t),
            Mathf.Lerp(a.height, b.height, t)
        );
    }

    public static Rect Resize(this Rect rect, float px)
    {
        rect.x += px;
        rect.y += px;
        rect.width -= px * 2;
        rect.height -= px * 2;
        return rect;
    }

    public static Rect SetPos(this Rect rect, Vector2 v) => rect.SetPos(v.x, v.y);

    public static Rect SetPos(this Rect rect, float x, float y)
    {
        rect.x = x;
        rect.y = y;
        return rect;
    }

    public static Rect SetX(this Rect rect, float x) => rect.SetPos(x, rect.y);
    public static Rect SetY(this Rect rect, float y) => rect.SetPos(rect.x, y);

    public static Rect SetXMax(this Rect rect, float xMax)
    {
        rect.xMax = xMax;
        return rect;
    }

    public static Rect SetYMax(this Rect rect, float yMax)
    {
        rect.yMax = yMax;
        return rect;
    }

    public static Rect SetMidPos(this Rect r, Vector2 v) => r.SetPos(v).MoveX(-r.width / 2).MoveY(-r.height / 2);
    public static Rect SetMidPos(this Rect r, float x, float y) => r.SetMidPos(new Vector2(x, y));

    public static Rect Move(this Rect rect, Vector2 v)
    {
        rect.position += v;
        return rect;
    }

    public static Rect Move(this Rect rect, float x, float y)
    {
        rect.x += x;
        rect.y += y;
        return rect;
    }

    public static Rect MoveX(this Rect rect, float px)
    {
        rect.x += px;
        return rect;
    }

    public static Rect MoveY(this Rect rect, float px)
    {
        rect.y += px;
        return rect;
    }

    public static Rect SetWidth(this Rect rect, float f)
    {
        rect.width = f;
        return rect;
    }

    public static Rect SetWidthFromMid(this Rect rect, float px)
    {
        rect.x += rect.width / 2;
        rect.width = px;
        rect.x -= rect.width / 2;
        return rect;
    }

    public static Rect SetWidthFromRight(this Rect rect, float px)
    {
        rect.x += rect.width;
        rect.width = px;
        rect.x -= rect.width;
        return rect;
    }

    public static Rect SetHeight(this Rect rect, float f)
    {
        rect.height = f;
        return rect;
    }

    public static Rect SetHeightFromMid(this Rect rect, float px)
    {
        rect.y += rect.height / 2;
        rect.height = px;
        rect.y -= rect.height / 2;
        return rect;
    }

    public static Rect SetHeightFromBottom(this Rect rect, float px)
    {
        rect.y += rect.height;
        rect.height = px;
        rect.y -= rect.height;
        return rect;
    }

    public static Rect AddWidth(this Rect rect, float f) => rect.SetWidth(rect.width + f);
    public static Rect AddWidthFromMid(this Rect rect, float f) => rect.SetWidthFromMid(rect.width + f);
    public static Rect AddWidthFromRight(this Rect rect, float f) => rect.SetWidthFromRight(rect.width + f);
    public static Rect AddHeight(this Rect rect, float f) => rect.SetHeight(rect.height + f);
    public static Rect AddHeightFromMid(this Rect rect, float f) => rect.SetHeightFromMid(rect.height + f);
    public static Rect AddHeightFromBottom(this Rect rect, float f) => rect.SetHeightFromBottom(rect.height + f);
    public static Rect SetSize(this Rect rect, Vector2 v) => rect.SetWidth(v.x).SetHeight(v.y);
    public static Rect SetSize(this Rect rect, float w, float h) => rect.SetWidth(w).SetHeight(h);

    public static Rect SetSize(this Rect rect, float f)
    {
        rect.height = rect.width = f;
        return rect;
    }

    public static Rect SetSizeFromMid(this Rect r, Vector2 v) => r.Move(r.size / 2).SetSize(v).Move(-v / 2);
    public static Rect SetSizeFromMid(this Rect r, float x, float y) => r.SetSizeFromMid(new Vector2(x, y));
    public static Rect SetSizeFromMid(this Rect r, float f) => r.SetSizeFromMid(new Vector2(f, f));
    public static Rect AlignToPixelGrid(this Rect r) => GUIUtility.AlignRectToDevice(r);

#if UNITY_EDITOR
    public static Rect Draw(this Rect rect, Color color)
    {
        EditorGUI.DrawRect(rect, color);
        return rect;
    }

    public static Rect Draw(this Rect rect) => rect.Draw(Color.black);

    public static Rect DrawOutline(this Rect rect, Color color, float thickness = 1)
    {
        rect.SetWidth(thickness).Draw(color);
        rect.SetWidthFromRight(thickness).Draw(color);
        rect.SetHeight(thickness).Draw(color);
        rect.SetHeightFromBottom(thickness).Draw(color);
        return rect;
    }

    public static Rect DrawOutline(this Rect rect, float thickness = 1) => rect.DrawOutline(Color.black, thickness);

    public static bool IsHovered(this Rect r) => r.Contains(Event.current.mousePosition);
#endif
}
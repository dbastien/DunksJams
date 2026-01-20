using System;
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
        {
            for (var x = 0; x < cols; ++x)
                rects[y * cols + x] = new(
                    r.xMin + x * cellWidth,
                    r.yMin + y * cellHeight,
                    cellWidth,
                    cellHeight);
        }
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
        return new(depthX, depthY);
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
        return new(r.center.x - newWidth / 2, r.center.y - newHeight / 2, newWidth, newHeight);
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
}
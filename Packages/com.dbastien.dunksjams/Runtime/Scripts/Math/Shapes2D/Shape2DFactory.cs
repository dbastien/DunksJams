using UnityEngine;

public static class Shape2DFactory
{
    public static Polygon2D Polygon(params Vector2[] vertices) => new Polygon2D { Vertices = vertices };

    public static Polygon2D RegularPolygon(Vector2 center, float radius, int sides, float rotationRadians = 0f)
    {
        sides = Mathf.Max(3, sides);
        var v = new Vector2[sides];
        float step = Mathf.PI * 2f / sides;

        for (int i = 0; i < sides; i++)
        {
            float a = rotationRadians + step * i;
            v[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }

        return new Polygon2D { Vertices = v };
    }

    public static Polygon2D StarPolygon(Vector2 center, float outerRadius, float innerRadius, int points, float rotationRadians = 0f)
    {
        points = Mathf.Max(2, points);
        int n = points * 2;
        var v = new Vector2[n];
        float step = Mathf.PI / points;

        for (int i = 0; i < n; i++)
        {
            float r = (i & 1) == 0 ? outerRadius : innerRadius;
            float a = rotationRadians + step * i;
            v[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
        }

        return new Polygon2D { Vertices = v };
    }

    public static Polygon2D Rectangle(Vector2 center, Vector2 size, float rotationRadians = 0f)
    {
        Vector2 h = size * 0.5f;
        var local = new Vector2[]
        {
            new(-h.x, -h.y),
            new( h.x, -h.y),
            new( h.x,  h.y),
            new(-h.x,  h.y),
        };

        return new Polygon2D { Vertices = Transform(local, center, rotationRadians) };
    }

    public static Polygon2D Parallelogram(Vector2 origin, Vector2 a, Vector2 b)
    {
        var v = new Vector2[4];
        v[0] = origin;
        v[1] = origin + a;
        v[2] = origin + a + b;
        v[3] = origin + b;
        return new Polygon2D { Vertices = v };
    }

    public static Polygon2D Trapezoid(Vector2 center, float topWidth, float bottomWidth, float height, float rotationRadians = 0f)
    {
        float hh = height * 0.5f;
        float ht = topWidth * 0.5f;
        float hb = bottomWidth * 0.5f;

        var local = new Vector2[]
        {
            new(-hb, -hh),
            new( hb, -hh),
            new( ht,  hh),
            new(-ht,  hh),
        };

        return new Polygon2D { Vertices = Transform(local, center, rotationRadians) };
    }

    public static Polygon2D Rhombus(Vector2 center, float diagonalX, float diagonalY, float rotationRadians = 0f)
    {
        float dx = diagonalX * 0.5f;
        float dy = diagonalY * 0.5f;

        var local = new Vector2[]
        {
            new( 0f, -dy),
            new( dx,  0f),
            new( 0f,  dy),
            new(-dx,  0f),
        };

        return new Polygon2D { Vertices = Transform(local, center, rotationRadians) };
    }

    public static Polygon2D Kite(Vector2 center, float top, float bottom, float left, float right, float rotationRadians = 0f)
    {
        var local = new Vector2[]
        {
            new( 0f,  top),
            new( right, 0f),
            new( 0f, -bottom),
            new(-left, 0f),
        };

        return new Polygon2D { Vertices = Transform(local, center, rotationRadians) };
    }

    public static Polygon2D RightTriangle(Vector2 rightAngleCorner, float width, float height, float rotationRadians = 0f)
    {
        var local = new Vector2[]
        {
            Vector2.zero,
            new(width, 0f),
            new(0f, height),
        };

        return new Polygon2D { Vertices = Transform(local, rightAngleCorner, rotationRadians) };
    }

    public static Polygon2D IsoscelesTriangle(Vector2 center, float baseWidth, float height, float rotationRadians = 0f)
    {
        float hb = baseWidth * 0.5f;
        float hh = height * 0.5f;

        var local = new Vector2[]
        {
            new(-hb, -hh),
            new( hb, -hh),
            new( 0f,  hh),
        };

        return new Polygon2D { Vertices = Transform(local, center, rotationRadians) };
    }

    public static Polygon2D Plus(Vector2 center, float size, float thickness, float rotationRadians = 0f)
    {
        float h = size * 0.5f;
        float t = thickness * 0.5f;

        var local = new Vector2[]
        {
            new(-t,  h),
            new( t,  h),
            new( t,  t),
            new( h,  t),
            new( h, -t),
            new( t, -t),
            new( t, -h),
            new(-t, -h),
            new(-t, -t),
            new(-h, -t),
            new(-h,  t),
            new(-t,  t),
        };

        return new Polygon2D { Vertices = Transform(local, center, rotationRadians) };
    }

    public static Polygon2D CrossX(Vector2 center, float size, float thickness, float rotationRadians = 0f)
        => Plus(center, size, thickness, rotationRadians + Mathf.PI * 0.25f);

    public static Polygon2D ArrowRight(Vector2 center, float length, float shaftThickness, float headLength, float headWidth, float rotationRadians = 0f)
    {
        float halfShaft = shaftThickness * 0.5f;
        float halfHeadW = headWidth * 0.5f;

        float x0 = -length * 0.5f;
        float x1 =  length * 0.5f;
        float xHeadBase = x1 - headLength;

        var local = new Vector2[]
        {
            new(x0,         halfShaft),
            new(xHeadBase,  halfShaft),
            new(xHeadBase,  halfHeadW),
            new(x1,         0f),
            new(xHeadBase, -halfHeadW),
            new(xHeadBase, -halfShaft),
            new(x0,        -halfShaft),
        };

        return new Polygon2D { Vertices = Transform(local, center, rotationRadians) };
    }

    public static Polygon2D ChevronRight(Vector2 center, float width, float height, float thickness, float rotationRadians = 0f)
    {
        float hw = width * 0.5f;
        float hh = height * 0.5f;
        float t = thickness * 0.5f;

        var local = new Vector2[]
        {
            new(-hw,  hh),
            new(-hw + thickness,  hh),
            new( hw,  0f),
            new(-hw + thickness, -hh),
            new(-hw, -hh),
            new( hw - thickness, 0f),
        };

        return new Polygon2D { Vertices = Transform(local, center, rotationRadians) };
    }

    public static Polygon2D Pie(Vector2 center, float radius, float startRadians, float sweepRadians, int segments)
    {
        segments = Mathf.Max(2, segments);

        var v = new Vector2[segments + 2];
        v[0] = center;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float a = startRadians + sweepRadians * t;
            v[i + 1] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }

        return new Polygon2D { Vertices = v };
    }

    public static Polygon2D RingSector(Vector2 center, float innerRadius, float outerRadius, float startRadians, float sweepRadians, int segments)
    {
        segments = Mathf.Max(2, segments);

        var v = new Vector2[(segments + 1) * 2];
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float a = startRadians + sweepRadians * t;
            Vector2 dir = new(Mathf.Cos(a), Mathf.Sin(a));
            v[i] = center + dir * outerRadius;
            v[v.Length - 1 - i] = center + dir * innerRadius;
        }

        return new Polygon2D { Vertices = v };
    }

    static Vector2[] Transform(Vector2[] local, Vector2 translate, float rotationRadians)
    {
        float c = Mathf.Cos(rotationRadians);
        float s = Mathf.Sin(rotationRadians);

        var v = new Vector2[local.Length];
        for (int i = 0; i < local.Length; i++)
        {
            Vector2 p = local[i];
            v[i] = new Vector2(p.x * c - p.y * s, p.x * s + p.y * c) + translate;
        }

        return v;
    }
}
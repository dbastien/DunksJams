using System;
using UnityEngine;

[Serializable]
public struct Polygon2D : IShape2D
{
    public Vector2[] Vertices;

    public bool Contains(Vector2 p)
    {
        var inside = false;
        for (int i = 0, j = Vertices.Length - 1; i < Vertices.Length; j = i++)
        {
            var vi = Vertices[i];
            var vj = Vertices[j];
            if (vi.y > p.y != vj.y > p.y &&
                p.x < (vj.x - vi.x) * (p.y - vi.y) / (vj.y - vi.y) + vi.x)
                inside = !inside;
        }

        return inside;
    }

    public Vector2 NearestPoint(Vector2 p)
    {
        var closest = Vertices[0];
        var minDistSq = float.MaxValue;

        for (var i = 0; i < Vertices.Length; ++i)
        {
            var a = Vertices[i];
            var b = Vertices[(i + 1) % Vertices.Length];
            var candidate = LineSegment2D.ClosestPointOnLineSegment(p, a, b);
            var distSq = (p - candidate).sqrMagnitude;

            if (distSq < minDistSq)
            {
                closest = candidate;
                minDistSq = distSq;
            }
        }

        return closest;
    }

    public void DrawGizmos()
    {
        for (var i = 0; i < Vertices.Length; ++i)
            Gizmos.DrawLine(Vertices[i], Vertices[(i + 1) % Vertices.Length]);
    }
}
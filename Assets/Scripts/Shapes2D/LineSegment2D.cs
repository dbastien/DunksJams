using UnityEngine;

[System.Serializable]
public struct LineSegment2D : IShape2D
{
    public Vector2 Start, End;

    public float Length => (End - Start).magnitude;

    public bool Contains(Vector2 p)
    {
        var d = End - Start;
        var t = Vector2.Dot(p - Start, d) / d.sqrMagnitude;
        return t is >= 0 and <= 1 && Mathf.Approximately((Start + t * d - p).sqrMagnitude, 0f);
    }

    public Vector2 NearestPoint(Vector2 p)
    {
        var d = End - Start;
        var t = Mathf.Clamp01(Vector2.Dot(p - Start, d) / d.sqrMagnitude);
        return Start + t * d;
    }

    public void DrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(Start, End);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Start, 0.05f);
        Gizmos.DrawSphere(End, 0.05f);
    }

    public bool Intersects(LineSegment2D other, out Vector2 inter)
    {
        var d1 = End - Start;
        var d2 = other.End - other.Start;
        var det = d1.x * d2.y - d1.y * d2.x;

        inter = Vector2.zero;

        if (Mathf.Abs(det) < Mathf.Epsilon) return false;

        var diff = other.Start - Start;
        var t = (diff.x * d2.y - diff.y * d2.x) / det;
        var u = (diff.x * d1.y - diff.y * d1.x) / det;

        if (t is >= 0 and <= 1 && u is >= 0 and <= 1)
        {
            inter = Start + t * d1;
            return true;
        }

        return false;
    }

    public bool Intersects(LineSegment2D other) => Intersects(other, out _);

    public static Vector2 ClosestPointOnLineSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        return a + ab * Mathf.Clamp01(t);
    }

    public static float DistanceToLineSegment(Vector2 p, Vector2 a, Vector2 b) =>
        Vector2.Distance(p, ClosestPointOnLineSegment(p, a, b));

    public static bool IsPointOnLineSegment(Vector2 p, Vector2 a, Vector2 b, float tol = 0.001f) =>
        DistanceToLineSegment(p, a, b) <= tol;
}
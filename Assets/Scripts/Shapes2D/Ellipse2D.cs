using UnityEngine;

[System.Serializable]
public struct Ellipse2D : IShape2D
{
    public Vector2 Center, Radii;

    public bool Contains(Vector2 p)
    {
        var d = p - Center;
        return d.x * d.x / (Radii.x * Radii.x) + d.y * d.y / (Radii.y * Radii.y) <= 1;
    }

    public Vector2 NearestPoint(Vector2 p)
    {
        var d = p - Center;
        var angle = Mathf.Atan2(d.y * Radii.x, d.x * Radii.y);
        return Center + new Vector2(Mathf.Cos(angle) * Radii.x, Mathf.Sin(angle) * Radii.y);
    }

    public void DrawGizmos()
    {
        const int seg = 36;
        var step = Mathf.Deg2Rad * 360f / seg;
        Vector3 prev = Center + new Vector2(Radii.x, 0), first = prev;

        for (int i = 1; i <= seg; ++i)
        {
            var a = i * step;
            var point = new Vector3(Mathf.Cos(a) * Radii.x + Center.x, Mathf.Sin(a) * Radii.y + Center.y, 0);
            Gizmos.DrawLine(prev, point);
            prev = point;
        }

        Gizmos.DrawLine(prev, first);
    }

    public bool Intersects(Ellipse2D e)
    {
        var d = (Center - e.Center).magnitude;
        var sum = (Radii.x + e.Radii.x + Radii.y + e.Radii.y) * 0.5f;
        return d <= sum;
    }
}
using System;
using UnityEngine;

[Serializable]
public struct Circle2D : IShape2D
{
    public Vector2 Center;
    public float Radius;

    public bool Contains(Vector2 p) => (Center - p).sqrMagnitude <= Radius * Radius;

    public Vector2 NearestPoint(Vector2 p)
    {
        Vector2 d = p - Center;
        return Center + d.normalized * Mathf.Min(Radius, d.magnitude);
    }

    public void DrawGizmos() => Gizmos.DrawWireSphere(Center, Radius);

    public bool Intersects(Circle2D c)
    {
        float rSum = Radius + c.Radius;
        return (Center - c.Center).sqrMagnitude <= rSum * rSum;
    }
}
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
		float mag = d.magnitude;
		if (mag <= 1e-8f) return Center + Vector2.right * Radius;

		float t = Mathf.Min(Radius, mag);
		return Center + d * (t / mag);
	}

    public void DrawGizmos() => Gizmos.DrawWireSphere(Center, Radius);

    public bool Intersects(Circle2D c)
    {
        float rSum = Radius + c.Radius;
        return (Center - c.Center).sqrMagnitude <= rSum * rSum;
    }
}
using System;
using UnityEngine;

[Serializable]
public struct AARect : IShape2D
{
    public Vector2 Min, Max;

    public bool Contains(Vector2 p) => p.x >= Min.x && p.x <= Max.x && p.y >= Min.y && p.y <= Max.y;

    public Vector2 NearestPoint(Vector2 p) => new(
        Mathf.Clamp(p.x, Min.x, Max.x),
        Mathf.Clamp(p.y, Min.y, Max.y)
    );

    public void DrawGizmos() => Gizmos.DrawWireCube((Min + Max) * 0.5f, Max - Min);

    public bool Intersects(AARect r) =>
        Min.x <= r.Max.x && Max.x >= r.Min.x && Min.y <= r.Max.y && Max.y >= r.Min.y;
}
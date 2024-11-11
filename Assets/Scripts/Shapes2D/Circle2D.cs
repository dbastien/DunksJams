using UnityEngine;

public struct Circle2D
{
    public Vector2 Center;
    public float Radius;

    float? _radiusSquared;

    public float RadiusSquared => _radiusSquared ??= Radius * Radius;
    
    public bool IsPointInside(Vector2 point)
    {
        float xD = Center.x - point.x;
        float yD = Center.y - point.y;

        float distanceSquared = xD * xD + yD * yD;
        return distanceSquared <= RadiusSquared;
    }
}

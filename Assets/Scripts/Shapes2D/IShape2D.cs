using UnityEngine;

public interface IShape2D
{
    bool Contains(Vector2 p);
    Vector2 NearestPoint(Vector2 p);
    void DrawGizmos();
}